using FluentValidation;
using MediatR;
using ShopFresherz.Application.Common;
using ShopFresherz.Domain.Entities;
using ShopFresherz.Domain.Interfaces;
using ShopFresherz.Domain.Interfaces.Services;

namespace ShopFresherz.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>Command for initiating a password reset flow via OTP email.</summary>
/// <param name="Request">The forgot-password payload.</param>
public sealed record ForgotPasswordCommand(Dtos.Auth.ForgotPasswordRequest Request) : IRequest<Result<bool>>;

/// <summary>Handler for <see cref="ForgotPasswordCommand"/>.</summary>
public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly IEmailService _email;

    /// <summary>Initialises the handler.</summary>
    public ForgotPasswordCommandHandler(IUnitOfWork uow, ICacheService cache, IEmailService email)
    {
        _uow = uow;
        _cache = cache;
        _email = email;
    }

    /// <inheritdoc />
    public async Task<Result<bool>> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        string email = command.Request.Email.Trim().ToLowerInvariant();

        User? user = await _uow.Users.GetByEmailAsync(email, cancellationToken);

        // Always return success to prevent email enumeration.
        if (user is null)
        {
            return Result<bool>.Success(true);
        }

        string otp = Random.Shared.Next(100_000, 999_999).ToString();
        await _cache.SetAsync(
            $"otp:reset:{email}",
            otp,
            TimeSpan.FromMinutes(10),
            cancellationToken);

        _ = _email.SendPasswordResetAsync(user.Email, user.FirstName, otp, CancellationToken.None);

        return Result<bool>.Success(true);
    }
}

/// <summary>Validator for <see cref="ForgotPasswordCommand"/>.</summary>
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
    }
}
