using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShopFresherz.Domain.Interfaces.Services;

namespace ShopFresherz.Infrastructure.Services;

/// <summary>
/// Paystack payment gateway implementation of <see cref="IPaymentService"/>.
/// Uses the Paystack v1 REST API (https://api.paystack.co).
/// Amounts are in kobo (NGN × 100) as required by Paystack.
/// </summary>
public sealed class PaystackPaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly string     _secretKey;
    private readonly ILogger<PaystackPaymentService> _logger;

    private const string BaseUrl = "https://api.paystack.co";

    /// <summary>Initialises a new instance of <see cref="PaystackPaymentService"/>.</summary>
    public PaystackPaymentService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PaystackPaymentService> logger)
    {
        _secretKey  = configuration["Paystack:SecretKey"]
            ?? throw new InvalidOperationException("Paystack:SecretKey is not configured.");
        _httpClient = httpClientFactory.CreateClient(nameof(PaystackPaymentService));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _secretKey);
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaymentInitResult> InitialiseAsync(
        string email,
        long amountKobo,
        string reference,
        string callbackUrl,
        CancellationToken cancellationToken = default)
    {
        object body = new
        {
            email        = email,
            amount       = amountKobo,
            reference    = reference,
            callback_url = callbackUrl,
            currency     = "NGN",
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"{BaseUrl}/transaction/initialize",
            body,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        PaystackInitResponse? result = await response.Content.ReadFromJsonAsync<PaystackInitResponse>(
            cancellationToken: cancellationToken);

        if (result?.Data == null)
        {
            throw new InvalidOperationException("Paystack returned an empty initialisation response.");
        }

        return new PaymentInitResult(result.Data.AuthorizationUrl, result.Data.Reference);
    }

    /// <inheritdoc />
    public async Task<PaymentVerifyResult> VerifyAsync(string reference, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(
            $"{BaseUrl}/transaction/verify/{Uri.EscapeDataString(reference)}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        PaystackVerifyResponse? result = await response.Content.ReadFromJsonAsync<PaystackVerifyResponse>(
            cancellationToken: cancellationToken);

        if (result?.Data == null)
        {
            return new PaymentVerifyResult(false, "failed", 0, reference);
        }

        bool isSuccessful = string.Equals(result.Data.Status, "success", StringComparison.OrdinalIgnoreCase);

        return new PaymentVerifyResult(
            isSuccessful,
            result.Data.Status,
            result.Data.Amount,
            result.Data.Reference);
    }

    /// <inheritdoc />
    public async Task<bool> RefundAsync(string reference, long amountKobo, CancellationToken cancellationToken = default)
    {
        object body = new
        {
            transaction       = reference,
            amount            = amountKobo,
            currency          = "NGN",
            customer_note     = "Refund processed by ShopFresherz",
            merchant_note     = "Auto-refund via API",
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"{BaseUrl}/refund",
            body,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Paystack refund for {Reference} failed with status {Status}.", reference, response.StatusCode);
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool VerifyWebhookSignature(string rawBody, string signature)
    {
        byte[] keyBytes    = Encoding.UTF8.GetBytes(_secretKey);
        byte[] bodyBytes   = Encoding.UTF8.GetBytes(rawBody);

        byte[] hash = HMACSHA512.HashData(keyBytes, bodyBytes);
        string computed = Convert.ToHexString(hash).ToLowerInvariant();

        return string.Equals(computed, signature, StringComparison.OrdinalIgnoreCase);
    }

    // ── Paystack response DTOs ─────────────────────────────────────────────────

    private sealed class PaystackInitResponse
    {
        [JsonPropertyName("data")]
        public PaystackInitData? Data { get; init; }
    }

    private sealed class PaystackInitData
    {
        [JsonPropertyName("authorization_url")]
        public string AuthorizationUrl { get; init; } = string.Empty;

        [JsonPropertyName("reference")]
        public string Reference { get; init; } = string.Empty;
    }

    private sealed class PaystackVerifyResponse
    {
        [JsonPropertyName("data")]
        public PaystackVerifyData? Data { get; init; }
    }

    private sealed class PaystackVerifyData
    {
        [JsonPropertyName("status")]
        public string Status { get; init; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; init; }

        [JsonPropertyName("reference")]
        public string Reference { get; init; } = string.Empty;
    }
}
