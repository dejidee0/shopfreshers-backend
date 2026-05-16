namespace ShopFresherz.Infrastructure.Configuration;

/// <summary>Configuration options for Paystack integration.</summary>
public sealed class PaystackOptions
{
    public string SecretKey { get; init; } = string.Empty;
    public string PublicKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
}
