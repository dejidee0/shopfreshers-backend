using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShopFresherz.Application.Features.Payments.Commands;
using ShopFresherz.Infrastructure.Configuration;

namespace ShopFresherz.API.Controllers;

/// <summary>Receives and verifies inbound payment gateway webhook events.</summary>
[ApiController]
[Route("api/v1/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly PaystackOptions _options;
    private readonly FlutterwaveOptions _flutterwaveOptions;

    public PaymentsController(
        IMediator mediator,
        IOptions<PaystackOptions> options,
        IOptions<FlutterwaveOptions> flutterwaveOptions)
    {
        _mediator = mediator;
        _options = options.Value;
        _flutterwaveOptions = flutterwaveOptions.Value;
    }

    /// <summary>
    /// Receives Paystack webhook events.
    /// Verifies the HMAC-SHA512 signature before processing.
    /// Always returns 200 OK to prevent Paystack retry storms.
    /// </summary>
    [HttpPost("webhook/paystack")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PaystackWebhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using StreamReader reader = new(Request.Body, Encoding.UTF8, leaveOpen: true);
        string rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        string? paystackSignature = Request.Headers["x-paystack-signature"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(paystackSignature))
        {
            return Ok();
        }

        string expectedHash = ComputeHmacSha512(rawBody, _options.WebhookSecret);
        if (!string.Equals(expectedHash, paystackSignature, StringComparison.OrdinalIgnoreCase))
        {
            return Ok();
        }

        using JsonDocument doc = JsonDocument.Parse(rawBody);
        JsonElement root = doc.RootElement;

        string eventName = root.GetProperty("event").GetString() ?? string.Empty;
        JsonElement data = root.GetProperty("data");
        string reference = data.GetProperty("reference").GetString() ?? string.Empty;
        decimal amountKobo = data.GetProperty("amount").GetDecimal();
        string email = data.GetProperty("customer")
            .GetProperty("email").GetString() ?? string.Empty;

        await _mediator.Send(
            new HandlePaystackWebhookCommand(eventName, reference, amountKobo, email),
            cancellationToken);

        return Ok();
    }

    /// <summary>Receives Flutterwave fallback payment webhook events.</summary>
    [HttpPost("webhook/flutterwave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> FlutterwaveWebhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using StreamReader reader = new(Request.Body, Encoding.UTF8, leaveOpen: true);
        string rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        string? signature = Request.Headers["verif-hash"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature) ||
            !string.Equals(signature, _flutterwaveOptions.WebhookSecret, StringComparison.Ordinal))
        {
            return Ok();
        }

        using JsonDocument doc = JsonDocument.Parse(rawBody);
        JsonElement root = doc.RootElement;

        string eventType = root.GetProperty("event").GetString() ?? string.Empty;
        if (!string.Equals(eventType, "charge.completed", StringComparison.OrdinalIgnoreCase))
        {
            return Ok();
        }

        JsonElement data = root.GetProperty("data");
        string reference = data.GetProperty("tx_ref").GetString() ?? string.Empty;
        decimal amount = data.GetProperty("amount").GetDecimal();
        string email = data.GetProperty("customer").GetProperty("email").GetString() ?? string.Empty;
        string status = data.GetProperty("status").GetString() ?? string.Empty;

        if (!string.Equals(status, "successful", StringComparison.OrdinalIgnoreCase))
        {
            return Ok();
        }

        await _mediator.Send(
            new HandlePaystackWebhookCommand("charge.success", reference, amount * 100, email),
            cancellationToken);

        return Ok();
    }

    private static string ComputeHmacSha512(string data, string secret)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] hashBytes = HMACSHA512.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
