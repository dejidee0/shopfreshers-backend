using MediatR;
using ShopFresherz.Application.Common;
using ShopFresherz.Domain.Entities;
using ShopFresherz.Domain.Enums;
using ShopFresherz.Domain.Interfaces;
using ShopFresherz.Domain.Interfaces.Services;

namespace ShopFresherz.Application.Features.Payments.Commands;

/// <summary>
/// Command for processing an inbound Paystack webhook event.
/// Stock is deducted and order status updated only after cryptographic signature verification in the controller.
/// </summary>
public sealed record HandlePaystackWebhookCommand(
    string Event,
    string Reference,
    decimal AmountKobo,
    string CustomerEmail) : IRequest<Result<bool>>;

/// <summary>Handler for <see cref="HandlePaystackWebhookCommand"/>.</summary>
public sealed class HandlePaystackWebhookCommandHandler
    : IRequestHandler<HandlePaystackWebhookCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;
    private readonly ISmsService _sms;

    public HandlePaystackWebhookCommandHandler(IUnitOfWork uow, IEmailService email, ISmsService sms)
    {
        _uow = uow;
        _email = email;
        _sms = sms;
    }

    /// <inheritdoc />
    public async Task<Result<bool>> Handle(
        HandlePaystackWebhookCommand command,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(command.Event, "charge.success", StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Success(true);
        }

        Order? order = await _uow.Orders.GetByPaymentReferenceAsync(
            command.Reference, cancellationToken);

        if (order is null || order.PaymentStatus == PaymentStatus.Paid)
        {
            return Result<bool>.Success(true);
        }

        foreach (OrderItem item in order.Items)
        {
            Product? product = await _uow.Products.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null) continue;

            if (item.VariantId.HasValue)
            {
                ProductVariant? variant = product.Variants
                    .FirstOrDefault(v => v.Id == item.VariantId);
                if (variant is not null)
                {
                    variant.StockQty = Math.Max(0, variant.StockQty - item.Quantity);
                    variant.ReservedQty = Math.Max(0, variant.ReservedQty - item.Quantity);
                    product.SoldCount += item.Quantity;
                }
            }
            else
            {
                product.StockQty = Math.Max(0, product.StockQty - item.Quantity);
                product.ReservedQty = Math.Max(0, product.ReservedQty - item.Quantity);
                product.SoldCount += item.Quantity;
            }

            _uow.Products.Update(product);
        }

        User? orderUser = null;
        if (order.UserId.HasValue)
        {
            orderUser = await _uow.Users.GetByIdAsync(order.UserId.Value, cancellationToken);
            if (orderUser is not null)
            {
                int pointsEarned = (int)Math.Floor(order.Total / 100m);
                orderUser.LoyaltyPoints += pointsEarned;

                await _uow.LoyaltyTransactions.AddAsync(new LoyaltyTransaction
                {
                    UserId = orderUser.Id,
                    EventType = LoyaltyEventType.Earned,
                    Points = pointsEarned,
                    Description = $"Earned from order {order.OrderNumber}",
                    OrderId = order.Id,
                    ExpiresAt = DateTime.UtcNow.AddYears(1),
                }, cancellationToken);

                _uow.Users.Update(orderUser);
            }
        }

        order.Status = OrderStatus.Processing;
        order.PaymentStatus = PaymentStatus.Paid;
        _uow.Orders.Update(order);

        await _uow.SaveChangesAsync(cancellationToken);

        string recipientEmail = orderUser?.Email ?? command.CustomerEmail;

        _ = _email.SendOrderConfirmationAsync(
            recipientEmail,
            orderUser?.FirstName ?? "Customer",
            order.OrderNumber,
            order.Total,
            CancellationToken.None);

        if (!string.IsNullOrWhiteSpace(orderUser?.Phone))
        {
            _ = _sms.SendOrderUpdateAsync(
                orderUser.Phone,
                order.OrderNumber,
                "confirmed & being processed",
                CancellationToken.None);
        }

        return Result<bool>.Success(true);
    }
}
