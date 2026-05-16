namespace ShopFresherz.Domain.Interfaces.Services;

/// <summary>
/// Contract for payment gateway integration.
/// Primary: Paystack. Fallback: Flutterwave.
/// Card data is never stored — tokenisation only (PCI DSS compliant).
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Initialises a payment transaction and returns the hosted checkout URL and reference.
    /// </summary>
    /// <param name="email">Customer email for the gateway.</param>
    /// <param name="amountKobo">Amount in kobo (NGN × 100).</param>
    /// <param name="reference">Unique order reference.</param>
    /// <param name="callbackUrl">URL for the gateway to redirect after payment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authorisation URL and the transaction reference.</returns>
    Task<PaymentInitResult> InitialiseAsync(
        string email,
        long amountKobo,
        string reference,
        string callbackUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a transaction status directly with the gateway (double-check after webhook).
    /// </summary>
    Task<PaymentVerifyResult> VerifyAsync(string reference, CancellationToken cancellationToken = default);

    /// <summary>Initiates a full or partial refund for a confirmed transaction.</summary>
    Task<bool> RefundAsync(string reference, long amountKobo, CancellationToken cancellationToken = default);

    /// <summary>Validates the HMAC-SHA512 webhook signature.</summary>
    bool VerifyWebhookSignature(string rawBody, string signature);
}

/// <summary>Result from payment initialisation.</summary>
public sealed record PaymentInitResult(string AuthorisationUrl, string Reference);

/// <summary>Result from payment verification.</summary>
public sealed record PaymentVerifyResult(bool IsSuccessful, string Status, long AmountKobo, string Reference);
