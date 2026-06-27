using MediatR;
using ShopFresherz.Application.Common;
using ShopFresherz.Application.Features.Payments.Services;
using ShopFresherz.Domain.Entities;
using ShopFresherz.Domain.Enums;
using ShopFresherz.Domain.Interfaces;

namespace ShopFresherz.Application.Features.Payments.Commands;

/// <summary>
/// Command for processing a verified successful payment webhook event.
/// Stock is deducted and order status updated only after gateway verification in the controller.
/// </summary>
public sealed record HandlePaymentWebhookCommand(
    string Event,
    string Reference,
    decimal AmountNgn,
    string Currency,
    string CustomerEmail) : IRequest<Result<bool>>;

/// <summary>Handler for <see cref="HandlePaymentWebhookCommand"/>.</summary>
public sealed class HandlePaymentWebhookCommandHandler
    : IRequestHandler<HandlePaymentWebhookCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly IOrderPaymentConfirmationService _paymentConfirmation;

    public HandlePaymentWebhookCommandHandler(
        IUnitOfWork uow,
        IOrderPaymentConfirmationService paymentConfirmation)
    {
        _uow = uow;
        _paymentConfirmation = paymentConfirmation;
    }

    /// <inheritdoc />
    public async Task<Result<bool>> Handle(
        HandlePaymentWebhookCommand command,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(command.Event, "charge.completed", StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Success(true);
        }

        // The two-step inline-popup flow stores the gateway reference in TxRef;
        // the legacy hosted-redirect flow stores the order number in PaymentReference.
        // Try TxRef first, then fall back so both flows keep working.
        Order? order =
            await _uow.Orders.GetByTxRefAsync(command.Reference, cancellationToken) ??
            await _uow.Orders.GetByPaymentReferenceAsync(command.Reference, cancellationToken);

        // Unknown reference: acknowledge so Flutterwave stops retrying.
        if (order is null)
        {
            return Result<bool>.Success(true);
        }

        if (order.PaymentStatus == PaymentStatus.Paid || order.Status == OrderStatus.Processing)
        {
            return Result<bool>.Success(true);
        }

        if (order.PaymentStatus != PaymentStatus.Unpaid ||
            order.Status is not (OrderStatus.Draft or OrderStatus.Pending or OrderStatus.AwaitingPayment))
        {
            return Result<bool>.Success(true);
        }

        if (command.AmountNgn < order.Total - 1m ||
            !string.Equals(command.Currency, "NGN", StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Success(true);
        }

        return await _paymentConfirmation.CompleteAsync(
            order,
            command.CustomerEmail,
            OrderStatus.Processing,
            cancellationToken);
    }
}
