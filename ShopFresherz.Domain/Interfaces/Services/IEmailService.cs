namespace ShopFresherz.Domain.Interfaces.Services;

/// <summary>
/// Contract for transactional email sending via SendGrid.
/// All methods are fire-and-monitor — they should not block the calling operation.
/// </summary>
public interface IEmailService
{
    /// <summary>Sends a 6-digit OTP for registration or password reset.</summary>
    Task SendOtpAsync(string toEmail, string firstName, string otp, CancellationToken cancellationToken = default);

    /// <summary>Sends an order placed confirmation email with order summary.</summary>
    Task SendOrderConfirmationAsync(string toEmail, string firstName, string orderNumber, decimal total, CancellationToken cancellationToken = default);

    /// <summary>Sends an order shipped notification with tracking number.</summary>
    Task SendOrderShippedAsync(string toEmail, string firstName, string orderNumber, string trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>Sends a password reset OTP email.</summary>
    Task SendPasswordResetAsync(string toEmail, string firstName, string otp, CancellationToken cancellationToken = default);

    /// <summary>Sends a back-in-stock notification for a product.</summary>
    Task SendBackInStockAsync(string toEmail, string firstName, string productName, string productSlug, CancellationToken cancellationToken = default);

    /// <summary>Sends a generic templated email by template key.</summary>
    Task SendTemplatedAsync(string toEmail, string templateKey, object templateData, CancellationToken cancellationToken = default);
}
