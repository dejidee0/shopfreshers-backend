namespace ShopFresherz.Domain.Enums;

/// <summary>Represents the payment state of an order.</summary>
public enum PaymentStatus
{
    /// <summary>No payment received yet.</summary>
    Unpaid = 0,

    /// <summary>Payment successfully confirmed by gateway webhook.</summary>
    Paid = 1,

    /// <summary>Payment has been refunded to customer.</summary>
    Refunded = 2
}
