namespace ShopFresherz.Domain.Interfaces.Services;

/// <summary>Fallback payment gateway integration for Flutterwave.</summary>
public interface IFlutterwavePaymentService
{
    Task<PaymentInitResult?> InitializeAsync(
        string email,
        Guid orderId,
        string orderNumber,
        decimal totalNgn,
        CancellationToken cancellationToken = default);
}
