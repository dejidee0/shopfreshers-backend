using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using ShopFresherz.Domain.Interfaces.Services;

namespace ShopFresherz.Infrastructure.Services;

/// <summary>
/// Transactional email service backed by SendGrid.
/// All email addresses and sender names are read from configuration.
/// </summary>
public sealed class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _client;
    private readonly EmailAddress    _from;
    private readonly ILogger<SendGridEmailService> _logger;

    /// <summary>Initialises a new instance of <see cref="SendGridEmailService"/>.</summary>
    public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
    {
        string? apiKey = configuration["SendGrid:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _client = null!;
            _isConfigured = false;
            _from = new EmailAddress("noreply@shopfresherz.com", "ShopFresherz");
            _logger = logger;
            return;
        }

        string fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@shopfresherz.com";
        string fromName  = configuration["SendGrid:FromName"]  ?? "ShopFresherz";

        _client = new SendGridClient(apiKey);
        _from   = new EmailAddress(fromEmail, fromName);
        _logger = logger;
        _isConfigured = true;
    }

    private readonly bool _isConfigured = true;

    /// <inheritdoc />
    public async Task SendOtpAsync(string toEmail, string firstName, string otp, CancellationToken cancellationToken = default)
    {
        string subject = "Your ShopFresherz verification code";
        string body    = $"Hi {firstName},<br/><br/>Your verification code is: <strong>{otp}</strong>.<br/>It expires in 10 minutes.<br/><br/>If you did not request this, please ignore this email.";

        await SendAsync(toEmail, firstName, subject, body, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendOrderConfirmationAsync(string toEmail, string firstName, string orderNumber, decimal total, CancellationToken cancellationToken = default)
    {
        string subject = $"Order Confirmed — {orderNumber}";
        string body    = $"Hi {firstName},<br/><br/>Your order <strong>{orderNumber}</strong> has been confirmed.<br/>Total charged: <strong>₦{total:N2}</strong>.<br/><br/>We'll send you another email when it ships. Thank you for shopping with ShopFresherz!";

        await SendAsync(toEmail, firstName, subject, body, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendOrderShippedAsync(string toEmail, string firstName, string orderNumber, string trackingNumber, CancellationToken cancellationToken = default)
    {
        string subject = $"Your Order {orderNumber} Has Shipped";
        string body    = $"Hi {firstName},<br/><br/>Great news! Your order <strong>{orderNumber}</strong> is on its way.<br/>Tracking number: <strong>{trackingNumber}</strong>.";

        await SendAsync(toEmail, firstName, subject, body, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendPasswordResetAsync(string toEmail, string firstName, string otp, CancellationToken cancellationToken = default)
    {
        string subject = "Reset Your ShopFresherz Password";
        string body    = $"Hi {firstName},<br/><br/>Use this code to reset your password: <strong>{otp}</strong>.<br/>It expires in 10 minutes.<br/><br/>If you did not request a reset, please ignore this email.";

        await SendAsync(toEmail, firstName, subject, body, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendBackInStockAsync(string toEmail, string firstName, string productName, string productSlug, CancellationToken cancellationToken = default)
    {
        string subject = $"{productName} is back in stock!";
        string body    = $"Hi {firstName},<br/><br/>Good news — <strong>{productName}</strong> is back in stock on ShopFresherz.<br/>Shop now before it sells out again!";

        await SendAsync(toEmail, firstName, subject, body, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendTemplatedAsync(string toEmail, string templateKey, object templateData, CancellationToken cancellationToken = default)
    {
        SendGridMessage message = new()
        {
            From       = _from,
            TemplateId = templateKey,
        };
        message.AddTo(new EmailAddress(toEmail));
        message.SetTemplateData(templateData);

        Response response = await _client.SendEmailAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("SendGrid templated email failed: {StatusCode}", response.StatusCode);
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        if (!_isConfigured)
        {
            _logger.LogInformation("SendGrid not configured — skipping email to {Email}", toEmail);
            return;
        }

        SendGridMessage message = MailHelper.CreateSingleEmail(
            from:    _from,
            to:      new EmailAddress(toEmail, toName),
            subject: subject,
            plainTextContent: null,
            htmlContent: htmlBody);

        Response response = await _client.SendEmailAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("SendGrid email to {Email} failed: {StatusCode}", toEmail, response.StatusCode);
        }
    }
}
