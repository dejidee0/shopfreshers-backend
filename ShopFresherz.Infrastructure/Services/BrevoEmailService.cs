using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopFresherz.Domain.Interfaces.Services;
using ShopFresherz.Infrastructure.Configuration;

namespace ShopFresherz.Infrastructure.Services;

/// <summary>Transactional email delivery through Brevo's REST API.</summary>
public sealed class BrevoEmailService : IEmailService
{
    private const string Endpoint = "https://api.brevo.com/v3/smtp/email";

    private readonly HttpClient _http;
    private readonly BrevoSettings _settings;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(
        HttpClient http,
        IOptions<BrevoSettings> settings,
        ILogger<BrevoEmailService> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task SendOtpAsync(string toEmail, string firstName, string otp, CancellationToken cancellationToken = default) =>
        SendAsync(toEmail, firstName, "Verify your ShopFresherz email",
            BrandedBody("Verify your email",
                $"<p>Hi {Encode(firstName)},</p><p>Use the verification code below to confirm your ShopFresherz email.</p>{OtpBlock(otp)}<p>This code expires in 10 minutes.</p>"),
            cancellationToken);

    public Task SendOrderConfirmationAsync(string toEmail, string firstName, string orderNumber, decimal total, CancellationToken cancellationToken = default) =>
        SendAsync(toEmail, firstName, $"Order Confirmed - {orderNumber}",
            BrandedBody("Order confirmed",
                $"<p>Hi {Encode(firstName)},</p><p>Thank you for shopping with ShopFresherz.</p>" +
                $"<p>Order: <strong>{Encode(orderNumber)}</strong><br/>Total: <strong>NGN {total:N2}</strong></p>" +
                "<p>We will notify you when your order status changes.</p>"),
            cancellationToken);

    public Task SendOrderShippedAsync(string toEmail, string firstName, string orderNumber, string trackingNumber, CancellationToken cancellationToken = default) =>
        SendAsync(toEmail, firstName, $"Your order {orderNumber} has shipped",
            BrandedBody("Your order is on its way",
                $"<p>Hi {Encode(firstName)},</p><p>Order <strong>{Encode(orderNumber)}</strong> has shipped.</p>" +
                $"<p>Tracking number: <strong>{Encode(trackingNumber)}</strong></p>"),
            cancellationToken);

    public Task SendPasswordResetAsync(string toEmail, string firstName, string otp, CancellationToken cancellationToken = default) =>
        SendAsync(toEmail, firstName, "Reset your ShopFresherz password",
            BrandedBody("Reset your password",
                $"<p>Hi {Encode(firstName)},</p><p>Use this six-digit code to reset your password.</p>{OtpBlock(otp)}" +
                "<p>This code expires in 15 minutes. If you did not request it, you can ignore this email.</p>"),
            cancellationToken);

    public Task SendBackInStockAsync(string toEmail, string firstName, string productName, string productSlug, CancellationToken cancellationToken = default) =>
        SendAsync(toEmail, firstName, $"{productName} is back in stock",
            BrandedBody("Back in stock",
                $"<p>Hi {Encode(firstName)},</p><p><strong>{Encode(productName)}</strong> is available again.</p>" +
                $"<p><a href=\"https://shopfresherz.com/products/{Uri.EscapeDataString(productSlug)}\">View product</a></p>"),
            cancellationToken);

    public Task SendTemplatedAsync(string toEmail, string templateKey, object templateData, CancellationToken cancellationToken = default) =>
        SendAsync(toEmail, string.Empty, templateKey,
            BrandedBody(Encode(templateKey), $"<pre>{Encode(JsonSerializer.Serialize(templateData))}</pre>"),
            cancellationToken);

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Brevo email skipped because recipient email is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey) || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            _logger.LogWarning("Brevo is not configured; email to {Email} was not sent.", toEmail);
            return;
        }

        try
        {
            using HttpRequestMessage request = new(HttpMethod.Post, Endpoint);
            request.Headers.Add("api-key", _settings.ApiKey);
            request.Headers.Accept.ParseAdd("application/json");
            request.Content = JsonContent.Create(new
            {
                sender = new { name = _settings.FromName, email = _settings.FromEmail },
                to = new[] { new { email = toEmail, name = toName } },
                subject,
                htmlContent = htmlBody,
            });

            using HttpResponseMessage response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Brevo email to {Email} failed with {StatusCode}: {ResponseBody}",
                    toEmail, response.StatusCode, responseBody);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Brevo email to {Email} timed out.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Brevo email to {Email} failed.", toEmail);
        }
    }

    private static string BrandedBody(string heading, string content) =>
        "<div style=\"font-family:Arial,sans-serif;max-width:600px;margin:auto;color:#17202a\">" +
        "<div style=\"background:#16794b;color:white;padding:20px;font-size:22px;font-weight:bold\">ShopFresherz</div>" +
        $"<div style=\"padding:24px;border:1px solid #e5e7eb\"><h2>{heading}</h2>{content}</div></div>";

    private static string OtpBlock(string otp) =>
        $"<div style=\"font-size:30px;font-weight:bold;letter-spacing:8px;padding:16px 0\">{Encode(otp)}</div>";

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
