using FluentValidation;
using MediatR;
using ShopFresherz.Application.Common;
using ShopFresherz.Domain.Entities;
using ShopFresherz.Domain.Interfaces;
using ShopFresherz.Domain.Interfaces.Services;

namespace ShopFresherz.Application.Features.Auth.Commands.ResetPassword;

/// <summary>Command for completing a password reset using the OTP sent to the user's email.</summary>
/// <param name="Request">The reset-password payload.</param>
public sealed record ResetPasswordCommand(Dtos.Auth.ResetPasswordRequest Request) : IRequest<Result<bool>>;

/// <summary>Handler for <see cref="ResetPasswordCommand"/>.</summary>
public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly IPasswordHasher _hasher;

    /// <summary>Initialises the handler.</summary>
    public ResetPasswordCommandHandler(IUnitOfWork uow, ICacheService cache, IPasswordHasher hasher)
    {
        _uow = uow;
        _cache = cache;
        _hasher = hasher;
    }

    /// <inheritdoc />
    public async Task<Result<bool>> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        string email = command.Request.Email.Trim().ToLowerInvariant();
        string cacheKey = $"otp:reset:{email}";

        string? storedOtp = await _cache.GetAsync<string>(cacheKey, cancellationToken);
        if (storedOtp is null)
        {
            return Error.Validation("Reset OTP has expired. Please request a new one.");
        }

        if (!string.Equals(storedOtp, command.Request.Otp.Trim(), StringComparison.Ordinal))
        {
            return Error.Validation("Invalid OTP.");
        }

        User? user = await _uow.Users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("User");
        }

        user.PasswordHash      = _hasher.Hash(command.Request.NewPassword);
        user.RefreshTokenHash   = null;
        user.RefreshTokenExpires = null;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(cacheKey, cancellationToken);

        return Result<bool>.Success(true);
    }
}

/// <summary>Validator for <see cref="ResetPasswordCommand"/>.</summary>
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    /// <summary>Initialises validation rules.</summary>
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Otp)
            .NotEmpty().Length(6).Matches(@"^\d{6}$").WithMessage("OTP must be 6 digits.");
        RuleFor(x => x.Request.NewPassword)
            .NotEmpty().MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.Request.ConfirmNewPassword)
            .Equal(x => x.Request.NewPassword).WithMessage("Passwords do not match.");
    }
}
