using System.Text.Json;
using FluentValidation;
using MediatR;
using ShopFresherz.Application.Common;
using ShopFresherz.Application.Dtos.Order;
using ShopFresherz.Domain.Entities;
using ShopFresherz.Domain.Enums;
using ShopFresherz.Domain.Interfaces;
using ShopFresherz.Domain.Interfaces.Services;
using CartEntity = ShopFresherz.Domain.Entities.Cart;

namespace ShopFresherz.Application.Features.Orders.Commands;

/// <summary>Command for converting the active cart into a confirmed order.</summary>
/// <param name="UserId">The authenticated user's ID (null for guest checkout).</param>
/// <param name="Request">The order creation payload.</param>
public sealed record PlaceOrderCommand(Guid? UserId, CreateOrderRequest Request)
    : IRequest<Result<CreateOrderResponse>>;

/// <summary>Handler for <see cref="PlaceOrderCommand"/>.</summary>
public sealed class PlaceOrderCommandHandler
    : IRequestHandler<PlaceOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentService _payment;
    private readonly IFlutterwavePaymentService _flutterwavePayment;
    private readonly IEmailService _email;

    /// <summary>Initialises the handler.</summary>
    public PlaceOrderCommandHandler(
        IUnitOfWork uow,
        IPaymentService payment,
        IFlutterwavePaymentService flutterwavePayment,
        IEmailService email)
    {
        _uow = uow;
        _payment = payment;
        _flutterwavePayment = flutterwavePayment;
        _email = email;
    }

    /// <inheritdoc />
    public async Task<Result<CreateOrderResponse>> Handle(
        PlaceOrderCommand command,
        CancellationToken cancellationToken)
    {
        CreateOrderRequest req = command.Request;

        // Resolve cart.
        CartEntity? cart = command.UserId.HasValue
            ? await _uow.Carts.GetByUserIdAsync(command.UserId.Value, cancellationToken)
            : !string.IsNullOrWhiteSpace(req.GuestSessionId)
                ? await _uow.Carts.GetBySessionIdAsync(req.GuestSessionId, cancellationToken)
                : null;

        if (cart is null || !cart.Items.Any())
        {
            return Error.Validation("Cart is empty. Add items before placing an order.");
        }

        // Resolve delivery address JSON snapshot.
        string addressJson;
        if (req.AddressId.HasValue)
        {
            Address? addr = await _uow.Addresses.GetByIdAsync(req.AddressId.Value, cancellationToken);
            if (addr is null) return Error.NotFound("Delivery address");
            addressJson = JsonSerializer.Serialize(new
            {
                addr.Label, addr.Line1, addr.Line2,
                addr.City, addr.State, addr.PostalCode,
            });
        }
        else if (req.InlineAddress is not null)
        {
            addressJson = JsonSerializer.Serialize(req.InlineAddress);
        }
        else
        {
            return Error.Validation("A delivery address is required.");
        }

        // Apply coupon.
        decimal discount = 0m;
        Coupon? coupon = null;
        if (!string.IsNullOrWhiteSpace(req.CouponCode))
        {
            coupon = await _uow.Coupons.GetByCodeAsync(req.CouponCode, cancellationToken);
            if (coupon is not null && coupon.IsActive && (coupon.ExpiresAt is null || coupon.ExpiresAt > DateTime.UtcNow))
            {
                decimal subtotal = cart.Items.Sum(i =>
                    (i.Variant?.Price ?? i.Product.Price) * i.Quantity);

                if (coupon.MinimumOrderAmount is null || subtotal >= coupon.MinimumOrderAmount)
                {
                    discount = coupon.Type == Domain.Enums.CouponType.Percentage
                        ? subtotal * (coupon.Value / 100m)
                        : coupon.Value;
                }
            }
        }

        // Build order items + reserve stock.
        List<OrderItem> items = new();
        decimal itemsSubtotal = 0m;

        foreach (CartItem ci in cart.Items)
        {
            Product? product = await _uow.Products.GetByIdWithLockAsync(ci.ProductId, cancellationToken);
            if (product is null) return Error.NotFound($"Product {ci.ProductId}");

            decimal unitPrice = ci.VariantId.HasValue
                ? product.Variants.FirstOrDefault(v => v.Id == ci.VariantId)?.Price ?? product.Price
                : product.Price;

            int availableQty = ci.VariantId.HasValue
                ? product.Variants.FirstOrDefault(v => v.Id == ci.VariantId)?.AvailableQty ?? 0
                : product.AvailableQty;

            if (availableQty < ci.Quantity)
            {
                return Error.Validation($"Insufficient stock for '{product.Name}'. Only {availableQty} unit(s) available.");
            }

            // Reserve stock.
            if (ci.VariantId.HasValue)
            {
                ProductVariant? variant = product.Variants.FirstOrDefault(v => v.Id == ci.VariantId);
                if (variant is not null) variant.ReservedQty += ci.Quantity;
            }
            else
            {
                product.ReservedQty += ci.Quantity;
            }

            _uow.Products.Update(product);

            decimal lineTotal = unitPrice * ci.Quantity;
            itemsSubtotal += lineTotal;

            items.Add(new OrderItem
            {
                ProductId   = ci.ProductId,
                VariantId   = ci.VariantId,
                Quantity    = ci.Quantity,
                UnitPrice   = unitPrice,
                LineTotal   = lineTotal,
                ProductSnapshotJson = JsonSerializer.Serialize(new
                {
                    Name   = product.Name,
                    SKU    = product.SKU,
                    Slug   = product.Slug,
                    ImageUrl = product.Images.OrderBy(img => img.SortOrder).FirstOrDefault()?.DisplayUrl,
                }),
            });
        }

        decimal deliveryFee = req.DeliveryMethod == DeliveryMethod.Express ? 3500m : 1500m;
        decimal vatAmount   = (itemsSubtotal - discount) * 0.075m;
        decimal total       = itemsSubtotal - discount + deliveryFee + vatAmount;

        string orderNumber = await _uow.Orders.GenerateOrderNumberAsync(cancellationToken);

        Order order = new()
        {
            OrderNumber         = orderNumber,
            UserId              = command.UserId,
            GuestEmail          = req.GuestEmail,
            Status              = OrderStatus.Pending,
            PaymentStatus       = PaymentStatus.Unpaid,
            PaymentMethod       = req.PaymentMethod,
            Subtotal            = itemsSubtotal,
            DiscountAmount      = discount,
            DeliveryFee         = deliveryFee,
            VatAmount           = vatAmount,
            Total               = total,
            CouponId            = coupon?.Id,
            DeliveryAddressJson = addressJson,
            DeliveryMethod      = req.DeliveryMethod,
            EstimatedDelivery   = DateTime.UtcNow.AddDays(req.DeliveryMethod == DeliveryMethod.Express ? 2 : 5),
            Notes               = req.Notes,
            Items               = items,
        };

        await _uow.Orders.AddAsync(order, cancellationToken);

        if (coupon is not null)
        {
            coupon.UsedCount++;
            _uow.Coupons.Update(coupon);
        }

        // Clear the cart.
        cart.Items.Clear();
        _uow.Carts.Update(cart);

        await _uow.SaveChangesAsync(cancellationToken);

        // Initiate payment.
        string? paymentUrl  = null;
        string? paymentRef  = null;

        if (req.PaymentMethod != PaymentMethod.PayOnDelivery)
        {
            string recipientEmail = command.UserId.HasValue
                ? (await _uow.Users.GetByIdAsync(command.UserId.Value, cancellationToken))?.Email ?? req.GuestEmail ?? string.Empty
                : req.GuestEmail ?? string.Empty;

            long amountKobo = (long)(total * 100);

            Domain.Interfaces.Services.PaymentInitResult? payResult = null;
            try
            {
                payResult = await _payment.InitialiseAsync(
                    recipientEmail,
                    amountKobo,
                    orderNumber,
                    $"https://shopfresherz.com/checkout/verify?ref={orderNumber}",
                    cancellationToken);
            }
            catch
            {
                payResult = null;
            }

            if (payResult is null)
            {
                payResult = await _flutterwavePayment.InitializeAsync(
                    recipientEmail,
                    order.Id,
                    orderNumber,
                    total,
                    cancellationToken);
            }

            if (payResult is null)
            {
                return Error.Internal("Payment initialisation failed. Please try again.");
            }

            order.PaymentReference = payResult.Reference;
            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync(cancellationToken);

            paymentUrl = payResult.AuthorisationUrl;
            paymentRef = payResult.Reference;
        }

        _ = _email.SendOrderConfirmationAsync(
            req.GuestEmail ?? string.Empty,
            "Customer",
            orderNumber,
            total,
            CancellationToken.None);

        return Result<CreateOrderResponse>.Success(new CreateOrderResponse
        {
            OrderId          = order.Id,
            OrderNumber      = orderNumber,
            PaymentUrl       = paymentUrl,
            PaymentReference = paymentRef,
            Total            = total,
        });
    }
}

/// <summary>Validator for <see cref="PlaceOrderCommand"/>.</summary>
public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.Request.PaymentMethod)
            .IsInEnum().WithMessage("A valid payment method is required.");

        RuleFor(x => x.Request.DeliveryMethod)
            .IsInEnum().WithMessage("A valid delivery method is required.");

        RuleFor(x => x.Request.GuestEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Request.GuestEmail));

        RuleFor(x => x)
            .Must(x => x.UserId.HasValue || !string.IsNullOrWhiteSpace(x.Request.GuestEmail))
            .WithMessage("Guest email is required for guest checkout.");
    }
}
