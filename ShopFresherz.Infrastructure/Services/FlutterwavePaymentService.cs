using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopFresherz.Domain.Interfaces.Services;
using ShopFresherz.Infrastructure.Configuration;

namespace ShopFresherz.Infrastructure.Services;

/// <summary>Fallback hosted checkout integration for Flutterwave.</summary>
public sealed class FlutterwavePaymentService : IFlutterwavePaymentService
{
    private readonly HttpClient _http;
    private readonly FlutterwaveOptions _options;
    private readonly ILogger<FlutterwavePaymentService> _logger;

    public FlutterwavePaymentService(
        HttpClient http,
        IOptions<FlutterwaveOptions> options,
        ILogger<FlutterwavePaymentService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Initialises a Flutterwave hosted payment and returns the payment link.</summary>
    public async Task<PaymentInitResult?> InitializeAsync(
        string email,
        Guid orderId,
        string orderNumber,
        decimal totalNgn,
        CancellationToken cancellationToken)
    {
        try
        {
            object payload = new
            {
                tx_ref = orderNumber,
                amount = totalNgn,
                currency = "NGN",
                redirect_url = _options.CallbackUrl,
                customer = new { email },
                customizations = new
                {
                    title = "ShopFresherz",
                    logo = "https://shopfresherz.com/logo.png",
                },
                meta = new { orderId = orderId.ToString() },
            };

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.SecretKey);

            HttpResponseMessage response = await _http.PostAsJsonAsync(
                "https://api.flutterwave.com/v3/payments",
                payload,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Flutterwave init failed for order {OrderNumber}: {Status} {Body}",
                    orderNumber,
                    response.StatusCode,
                    body);
                return null;
            }

            using JsonDocument doc = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken));

            string? link = doc.RootElement
                .GetProperty("data")
                .GetProperty("link")
                .GetString();

            return string.IsNullOrWhiteSpace(link)
                ? null
                : new PaymentInitResult(link, orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flutterwave exception for order {OrderNumber}", orderNumber);
            return null;
        }
    }
}
