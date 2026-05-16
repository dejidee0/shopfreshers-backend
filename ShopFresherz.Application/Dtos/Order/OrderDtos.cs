using ShopFresherz.Domain.Enums;

namespace ShopFresherz.Application.Dtos.Order;

/// <summary>Snapshot of the delivery address embedded in an order response.</summary>
public sealed class DeliveryAddressSnapshot
{
    /// <summary>Gets or sets the friendly label (e.g., "Home").</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the first address line.</summary>
    public string Line1 { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional second address line.</summary>
    public string? Line2 { get; set; }

    /// <summary>Gets or sets the city.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the Nigerian state.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets the postal code.</summary>
    public string? PostalCode { get; set; }
}

/// <summary>Snapshot of the product embedded in an order line item.</summary>
public sealed class ProductSnapshot
{
    /// <summary>Gets or sets the product name at order time.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the product SKU at order time.</summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary 540px image URL at order time.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the product slug at order time.</summary>
    public string Slug { get; set; } = string.Empty;
}

/// <summary>Order line item DTO.</summary>
public sealed class OrderItemDto
{
    /// <summary>Gets or sets the order item ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the selected variant ID.</summary>
    public Guid? VariantId { get; set; }

    /// <summary>Gets or sets the product snapshot at time of order.</summary>
    public ProductSnapshot? ProductSnapshot { get; set; }

    /// <summary>Gets or sets the quantity ordered.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the unit price at order time.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Gets or sets the computed line total.</summary>
    public decimal LineTotal { get; set; }
}

/// <summary>Full order DTO returned to clients and admins.</summary>
public sealed class OrderDto
{
    /// <summary>Gets or sets the order ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the human-readable order reference.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the owning user ID.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Gets or sets the current order status.</summary>
    public OrderStatus Status { get; set; }

    /// <summary>Gets or sets the payment status.</summary>
    public PaymentStatus PaymentStatus { get; set; }

    /// <summary>Gets or sets the payment method.</summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>Gets or sets the subtotal before discounts.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Gets or sets the discount amount.</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Gets or sets the delivery fee.</summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>Gets or sets the VAT amount (7.5%).</summary>
    public decimal VatAmount { get; set; }

    /// <summary>Gets or sets the final total.</summary>
    public decimal Total { get; set; }

    /// <summary>Gets or sets the delivery address snapshot.</summary>
    public DeliveryAddressSnapshot? DeliveryAddress { get; set; }

    /// <summary>Gets or sets the selected delivery method.</summary>
    public DeliveryMethod DeliveryMethod { get; set; }

    /// <summary>Gets or sets the estimated delivery date.</summary>
    public DateTime? EstimatedDelivery { get; set; }

    /// <summary>Gets or sets the courier tracking number.</summary>
    public string? TrackingNumber { get; set; }

    /// <summary>Gets or sets the customer order note.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the order line items.</summary>
    public IReadOnlyList<OrderItemDto> Items { get; set; } = [];

    /// <summary>Gets or sets the UTC timestamp when the order was placed.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Request payload for creating a new order from the active cart.</summary>
public sealed class CreateOrderRequest
{
    /// <summary>Gets or sets the saved address ID to deliver to (mutually exclusive with InlineAddress).</summary>
    public Guid? AddressId { get; set; }

    /// <summary>Gets or sets an inline address (mutually exclusive with AddressId).</summary>
    public InlineAddressRequest? InlineAddress { get; set; }

    /// <summary>Gets or sets the selected delivery method.</summary>
    public DeliveryMethod DeliveryMethod { get; set; }

    /// <summary>Gets or sets the selected payment method.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Gets or sets the coupon code to apply (optional).</summary>
    public string? CouponCode { get; set; }

    /// <summary>Gets or sets optional customer notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the guest email for guest checkout (required if not authenticated).</summary>
    public string? GuestEmail { get; set; }

    /// <summary>Gets or sets the guest session ID for guest checkout.</summary>
    public string? GuestSessionId { get; set; }
}

/// <summary>Inline address for one-time checkout without saving to profile.</summary>
public sealed class InlineAddressRequest
{
    /// <summary>Gets or sets the address label.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the first address line.</summary>
    public string Line1 { get; set; } = string.Empty;

    /// <summary>Gets or sets the second address line.</summary>
    public string? Line2 { get; set; }

    /// <summary>Gets or sets the city.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the Nigerian state.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Gets or sets the postal code.</summary>
    public string? PostalCode { get; set; }
}

/// <summary>Response returned after a successful order creation.</summary>
public sealed class CreateOrderResponse
{
    /// <summary>Gets or sets the created order ID.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the order number.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the Paystack authorisation URL to redirect the customer to.</summary>
    public string? PaymentUrl { get; set; }

    /// <summary>Gets or sets the payment reference for subsequent verification.</summary>
    public string? PaymentReference { get; set; }

    /// <summary>Gets or sets the order total.</summary>
    public decimal Total { get; set; }
}

/// <summary>Request payload for updating order status (admin).</summary>
public sealed class UpdateOrderStatusRequest
{
    /// <summary>Gets or sets the new order status.</summary>
    public OrderStatus Status { get; set; }

    /// <summary>Gets or sets the courier tracking number (required when transitioning to Shipped).</summary>
    public string? TrackingNumber { get; set; }

    /// <summary>Gets or sets the estimated delivery date.</summary>
    public DateTime? EstimatedDelivery { get; set; }
}
