using MediatR;
using ShopFresherz.Application.Common;
using ShopFresherz.Application.Dtos.Promotions;
using ShopFresherz.Domain.Entities;
using ShopFresherz.Domain.Interfaces;

namespace ShopFresherz.Application.Features.Promotions.Commands;

// ── Shared helper ─────────────────────────────────────────────────────────────

file static class PriceFormatter
{
    /// <summary>Formats a decimal price as a display string, e.g. "₦1,099".</summary>
    internal static string Format(decimal price) => $"₦{price:N0}";

    /// <summary>Formats a hero price, e.g. "From ₦1,099".</summary>
    internal static string FormatFrom(decimal price) => $"From ₦{price:N0}";

    /// <summary>Computes a discount badge text when a compare-at price is available.</summary>
    internal static string? DiscountBadge(decimal? compareAt, decimal price)
    {
        if (compareAt is null || compareAt <= price) return null;
        int pct = (int)Math.Round((compareAt.Value - price) / compareAt.Value * 100m);
        return pct > 0 ? $"{pct}% OFF" : null;
    }
}

file static class AdminPromotionMapper
{
    internal static AdminHeroBannerDto ToHero(PromotionalSection section) => new(
        section.Id,
        section.ProductId,
        section.SlugId,
        section.Title,
        section.Tag,
        section.Badge,
        section.CtaText,
        section.ImageUrl,
        section.PriceText,
        section.Slug,
        section.SortOrder,
        section.IsActive,
        section.CreatedAt);

    internal static AdminBestDealDto ToBestDeal(PromotionalSection section) => new(
        section.Id,
        section.ProductId,
        section.SlugId,
        section.Title,
        section.ImageUrl,
        section.SalePriceText,
        section.Badge,
        section.Slug,
        section.SortOrder,
        section.IsActive,
        section.CreatedAt);
}

// ── Set Hero Banner ───────────────────────────────────────────────────────────

/// <summary>Adds a new hero banner slide linked to an existing product.</summary>
public sealed record SetHeroBannerCommand(SetHeroBannerRequest Request)
    : IRequest<Result<AdminHeroBannerDto>>;

/// <summary>Handler for <see cref="SetHeroBannerCommand"/>.</summary>
public sealed class SetHeroBannerCommandHandler
    : IRequestHandler<SetHeroBannerCommand, Result<AdminHeroBannerDto>>
{
    private readonly IUnitOfWork _uow;

    public SetHeroBannerCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<AdminHeroBannerDto>> Handle(
        SetHeroBannerCommand cmd, CancellationToken cancellationToken)
    {
        if (cmd.Request.ProductId == Guid.Empty)
            return Error.Validation("A valid productId is required.");

        Product? product = await _uow.Products.GetByIdAsync(cmd.Request.ProductId, cancellationToken);
        if (product is null)
            return Result<AdminHeroBannerDto>.Failure(new Error("NOT_FOUND", "Product not found"));

        string slugId = $"hero-{product.Slug}";

        PromotionalSection? section = await _uow.PromotionalSections
            .GetByProductIdAsync("hero", product.Id, cancellationToken);
        section ??= await _uow.PromotionalSections.GetBySlugIdAsync(slugId, cancellationToken);

        if (section is null)
        {
            section = new PromotionalSection
            {
                SectionKey = "hero",
                ContentType = "hero-banner",
                SlugId = slugId,
            };
            await _uow.PromotionalSections.AddAsync(section, cancellationToken);
        }

        section.ProductId = product.Id;
        section.SlugId = slugId;
        section.Slug = product.Slug;
        section.Tag = cmd.Request.Tag?.Trim();
        section.Badge = cmd.Request.Badge?.Trim();
        section.Title = product.Name;
        section.PriceText = PriceFormatter.FormatFrom(product.Price);
        section.CtaText = cmd.Request.CtaText?.Trim();
        section.ImageUrl = product.ImageUrls.FirstOrDefault();
        section.SortOrder = cmd.Request.SortOrder;
        section.IsActive = true;
        section.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);

        return Result<AdminHeroBannerDto>.Success(AdminPromotionMapper.ToHero(section));
    }
}

// ── Set Best Deal ─────────────────────────────────────────────────────────────

/// <summary>Replaces the single "Best Deal" featured product.</summary>
public sealed record SetBestDealCommand(SetBestDealRequest Request)
    : IRequest<Result<AdminBestDealDto>>;

/// <summary>Handler for <see cref="SetBestDealCommand"/>.</summary>
public sealed class SetBestDealCommandHandler
    : IRequestHandler<SetBestDealCommand, Result<AdminBestDealDto>>
{
    private readonly IUnitOfWork _uow;

    public SetBestDealCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<AdminBestDealDto>> Handle(
        SetBestDealCommand cmd, CancellationToken cancellationToken)
    {
        if (cmd.Request.ProductId == Guid.Empty)
            return Error.Validation("A valid productId is required.");

        Product? product = await _uow.Products.GetByIdAsync(cmd.Request.ProductId, cancellationToken);
        if (product is null)
            return Result<AdminBestDealDto>.Failure(new Error("NOT_FOUND", "Product not found"));

        string slugId = $"deal-{product.Slug}";
        PromotionalSection? section = await _uow.PromotionalSections
            .GetByProductIdAsync("best-deal", product.Id, cancellationToken);
        section ??= await _uow.PromotionalSections.GetBySlugIdAsync(slugId, cancellationToken);

        if (section is null)
        {
            section = new PromotionalSection
            {
                SectionKey = "best-deal",
                ContentType = "best-deal-card",
                SlugId = slugId,
            };
            await _uow.PromotionalSections.AddAsync(section, cancellationToken);
        }

        section.ProductId = product.Id;
        section.SlugId = slugId;
        section.Slug = product.Slug;
        section.Title = product.Name;
        section.Badge = cmd.Request.Badge?.Trim();
        section.Rating = product.AverageRating > 0 ? product.AverageRating : null;
        section.OriginalPriceText = product.CompareAtPrice.HasValue
            ? PriceFormatter.Format(product.CompareAtPrice.Value)
            : null;
        section.SalePriceText = PriceFormatter.Format(product.Price);
        section.Description = product.ShortDescription;
        section.ImageUrl = product.ImageUrls.FirstOrDefault();
        section.SortOrder = cmd.Request.SortOrder;
        section.IsActive = true;
        section.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);

        return Result<AdminBestDealDto>.Success(AdminPromotionMapper.ToBestDeal(section));
    }
}

// ── Set Promo Banner ──────────────────────────────────────────────────────────

/// <summary>Replaces the full-width promotional section banner.</summary>
public sealed record SetPromoBannerCommand(SetPromoBannerRequest Request)
    : IRequest<Result<Guid>>;

/// <summary>Handler for <see cref="SetPromoBannerCommand"/>.</summary>
public sealed class SetPromoBannerCommandHandler
    : IRequestHandler<SetPromoBannerCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public SetPromoBannerCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(
        SetPromoBannerCommand cmd, CancellationToken cancellationToken)
    {
        await _uow.PromotionalSections.DeleteBySectionKeyAsync("promo-banner", cancellationToken);

        string slugId = $"promo-banner-{Guid.NewGuid():N}"[..30];

        var section = new PromotionalSection
        {
            SectionKey  = "promo-banner",
            ContentType = "promo-banner",
            SlugId      = slugId,
            Title       = cmd.Request.Title,
            Subtitle    = cmd.Request.Subtitle,
            CtaText     = cmd.Request.CtaText,
            ImageUrl    = cmd.Request.ImageUrl,
            ImageAlt    = cmd.Request.ImageAlt,
            Badge       = cmd.Request.Badge,
            SortOrder   = 1,
            IsActive    = true,
        };

        await _uow.PromotionalSections.AddAsync(section, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(section.Id);
    }
}

// ── Set Accessories Promo ─────────────────────────────────────────────────────

/// <summary>Replaces the Computer Accessories featured product card.</summary>
public sealed record SetAccessoriesPromoCommand(SetAccessoriesPromoRequest Request)
    : IRequest<Result<Guid>>;

/// <summary>Handler for <see cref="SetAccessoriesPromoCommand"/>.</summary>
public sealed class SetAccessoriesPromoCommandHandler
    : IRequestHandler<SetAccessoriesPromoCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public SetAccessoriesPromoCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(
        SetAccessoriesPromoCommand cmd, CancellationToken cancellationToken)
    {
        Product? product = await _uow.Products.GetByIdAsync(cmd.Request.ProductId, cancellationToken);
        if (product is null)
            return Result<Guid>.Failure(new Error("NOT_FOUND", "Product not found."));

        await _uow.PromotionalSections.DeleteBySectionKeyAsync("accessories-promo", cancellationToken);

        var section = new PromotionalSection
        {
            SectionKey  = "accessories-promo",
            ContentType = "feature-card",
            ProductId   = product.Id,
            SlugId      = $"accessories-{product.Slug}",
            Slug        = product.Slug,
            Title       = product.Name,
            Subtitle    = product.ShortDescription,
            ButtonText  = cmd.Request.CtaText,
            ImageUrl    = product.ImageUrls.FirstOrDefault(),
            PriceLabel  = "Price",
            PriceValue  = PriceFormatter.Format(product.Price),
            SortOrder   = 1,
            IsActive    = true,
        };

        await _uow.PromotionalSections.AddAsync(section, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(section.Id);
    }
}

// ── Set Laptop / Bottom Promo ─────────────────────────────────────────────────

/// <summary>Replaces the bottom promo card shown below the Computer Accessories section.</summary>
public sealed record SetLaptopPromoCommand(SetLaptopPromoRequest Request)
    : IRequest<Result<Guid>>;

/// <summary>Handler for <see cref="SetLaptopPromoCommand"/>.</summary>
public sealed class SetLaptopPromoCommandHandler
    : IRequestHandler<SetLaptopPromoCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public SetLaptopPromoCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(
        SetLaptopPromoCommand cmd, CancellationToken cancellationToken)
    {
        Product? product = await _uow.Products.GetByIdAsync(cmd.Request.ProductId, cancellationToken);
        if (product is null)
            return Result<Guid>.Failure(new Error("NOT_FOUND", "Product not found."));

        await _uow.PromotionalSections.DeleteBySectionKeyAsync("laptop-promo", cancellationToken);

        string? badge = PriceFormatter.DiscountBadge(product.CompareAtPrice, product.Price);

        var section = new PromotionalSection
        {
            SectionKey  = "laptop-promo",
            ContentType = "laptop-promo",
            ProductId   = product.Id,
            SlugId      = $"laptop-promo-{product.Slug}",
            Slug        = product.Slug,
            Title       = product.Name,
            Subtitle    = product.ShortDescription,
            CtaText     = cmd.Request.CtaText,
            ImageUrl    = product.ImageUrls.FirstOrDefault(),
            ImageAlt    = product.Name,
            PriceBadge  = badge ?? "From",
            PriceValue  = PriceFormatter.Format(product.Price),
            SortOrder   = 1,
            IsActive    = true,
        };

        await _uow.PromotionalSections.AddAsync(section, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(section.Id);
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  DELETE
// ═════════════════════════════════════════════════════════════════════════════

public sealed record DeletePromotionalSectionCommand(Guid Id) : IRequest<Result<bool>>;

public sealed class DeletePromotionalSectionCommandHandler
    : IRequestHandler<DeletePromotionalSectionCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public DeletePromotionalSectionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(
        DeletePromotionalSectionCommand cmd, CancellationToken cancellationToken)
    {
        PromotionalSection? section =
            await _uow.PromotionalSections.GetByIdAsync(cmd.Id, cancellationToken);
        if (section is null) return Error.NotFound("Promotional section");

        section.DeletedAt = DateTime.UtcNow;
        section.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  UPDATE COMMANDS
// ═════════════════════════════════════════════════════════════════════════════

public sealed record UpdateHeroBannerCommand(string SlugId, UpdateHeroBannerRequest Request)
    : IRequest<Result<AdminHeroBannerDto>>;

public sealed class UpdateHeroBannerCommandHandler
    : IRequestHandler<UpdateHeroBannerCommand, Result<AdminHeroBannerDto>>
{
    private readonly IUnitOfWork _uow;
    public UpdateHeroBannerCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<AdminHeroBannerDto>> Handle(
        UpdateHeroBannerCommand cmd,
        CancellationToken cancellationToken)
    {
        PromotionalSection? section = await _uow.PromotionalSections
            .GetBySlugIdAsync(cmd.SlugId, cancellationToken);
        if (section is null) return Error.NotFound("Hero banner");
        if (!string.Equals(section.SectionKey, "hero", StringComparison.Ordinal))
            return Error.NotFound("Hero banner");

        if (cmd.Request.ProductId is { } pid && pid != Guid.Empty)
        {
            Product? product = await _uow.Products.GetByIdAsync(pid, cancellationToken);
            if (product is null) return Error.NotFound("Product");
            section.ProductId = product.Id;
            section.SlugId    = $"hero-{product.Slug}";
            section.Slug      = product.Slug;
            section.Title     = product.Name;
            section.PriceText = PriceFormatter.FormatFrom(product.Price);
            section.ImageUrl  = product.ImageUrls.FirstOrDefault();
        }
        if (cmd.Request.Tag is not null)     section.Tag     = cmd.Request.Tag.Trim();
        if (cmd.Request.Badge is not null)   section.Badge   = cmd.Request.Badge.Trim();
        if (cmd.Request.CtaText is not null) section.CtaText = cmd.Request.CtaText.Trim();
        if (cmd.Request.SortOrder.HasValue)  section.SortOrder = cmd.Request.SortOrder.Value;
        if (cmd.Request.IsActive.HasValue)   section.IsActive = cmd.Request.IsActive.Value;

        section.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);
        return Result<AdminHeroBannerDto>.Success(AdminPromotionMapper.ToHero(section));
    }
}

public sealed record DisableHeroBannerCommand(string SlugId) : IRequest<Result<bool>>;

public sealed class DisableHeroBannerCommandHandler
    : IRequestHandler<DisableHeroBannerCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public DisableHeroBannerCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(
        DisableHeroBannerCommand cmd,
        CancellationToken cancellationToken)
    {
        PromotionalSection? section = await _uow.PromotionalSections
            .GetBySlugIdAsync(cmd.SlugId, cancellationToken);
        if (section is null || !string.Equals(section.SectionKey, "hero", StringComparison.Ordinal))
            return Error.NotFound("Hero banner");

        section.IsActive = false;
        section.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

public sealed record UpdateBestDealCommand(Guid Id, UpdateBestDealRequest Request) : IRequest<Result<bool>>;

public sealed class UpdateBestDealCommandHandler : IRequestHandler<UpdateBestDealCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public UpdateBestDealCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(UpdateBestDealCommand cmd, CancellationToken cancellationToken)
    {
        PromotionalSection? section = await _uow.PromotionalSections.GetByIdAsync(cmd.Id, cancellationToken);
        if (section is null) return Error.NotFound("Best deal card");

        if (cmd.Request.ProductId is { } pid && pid != Guid.Empty)
        {
            Product? product = await _uow.Products.GetByIdAsync(pid, cancellationToken);
            if (product is null) return Error.NotFound("Product");
            section.ProductId         = product.Id;
            section.SlugId            = $"deal-{product.Slug}";
            section.Slug              = product.Slug;
            section.Title             = product.Name;
            section.ImageUrl          = product.ImageUrls.FirstOrDefault();
            section.Rating            = product.AverageRating > 0 ? product.AverageRating : null;
            section.OriginalPriceText = product.CompareAtPrice.HasValue ? PriceFormatter.Format(product.CompareAtPrice.Value) : null;
            section.SalePriceText     = PriceFormatter.Format(product.Price);
            section.Description       = product.ShortDescription;
            section.Badge             = PriceFormatter.DiscountBadge(product.CompareAtPrice, product.Price);
        }
        section.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

public sealed record UpdatePromoBannerCommand(Guid Id, UpdatePromoBannerRequest Request) : IRequest<Result<bool>>;

public sealed class UpdatePromoBannerCommandHandler : IRequestHandler<UpdatePromoBannerCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public UpdatePromoBannerCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(UpdatePromoBannerCommand cmd, CancellationToken cancellationToken)
    {
        PromotionalSection? section = await _uow.PromotionalSections.GetByIdAsync(cmd.Id, cancellationToken);
        if (section is null) return Error.NotFound("Promo banner");

        UpdatePromoBannerRequest req = cmd.Request;
        if (req.Title is not null)    section.Title    = req.Title.Trim();
        if (req.Subtitle is not null) section.Subtitle = req.Subtitle.Trim();
        if (req.CtaText is not null)  section.CtaText  = req.CtaText.Trim();
        if (req.ImageUrl is not null) section.ImageUrl = req.ImageUrl.Trim();
        if (req.ImageAlt is not null) section.ImageAlt = req.ImageAlt.Trim();
        if (req.Badge is not null)    section.Badge    = req.Badge.Trim();

        section.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

public sealed record UpdateAccessoriesPromoCommand(Guid Id, UpdateAccessoriesPromoRequest Request) : IRequest<Result<bool>>;

public sealed class UpdateAccessoriesPromoCommandHandler : IRequestHandler<UpdateAccessoriesPromoCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public UpdateAccessoriesPromoCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(UpdateAccessoriesPromoCommand cmd, CancellationToken cancellationToken)
    {
        PromotionalSection? section = await _uow.PromotionalSections.GetByIdAsync(cmd.Id, cancellationToken);
        if (section is null) return Error.NotFound("Accessories promo");

        if (cmd.Request.ProductId is { } pid && pid != Guid.Empty)
        {
            Product? product = await _uow.Products.GetByIdAsync(pid, cancellationToken);
            if (product is null) return Error.NotFound("Product");
            section.ProductId  = product.Id;
            section.SlugId     = $"accessories-{product.Slug}";
            section.Slug       = product.Slug;
            section.Title      = product.Name;
            section.Subtitle   = product.ShortDescription;
            section.ImageUrl   = product.ImageUrls.FirstOrDefault();
            section.PriceValue = PriceFormatter.Format(product.Price);
        }
        if (cmd.Request.CtaText is not null) section.ButtonText = cmd.Request.CtaText.Trim();

        section.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

public sealed record UpdateLaptopPromoCommand(Guid Id, UpdateLaptopPromoRequest Request) : IRequest<Result<bool>>;

public sealed class UpdateLaptopPromoCommandHandler : IRequestHandler<UpdateLaptopPromoCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public UpdateLaptopPromoCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(UpdateLaptopPromoCommand cmd, CancellationToken cancellationToken)
    {
        PromotionalSection? section = await _uow.PromotionalSections.GetByIdAsync(cmd.Id, cancellationToken);
        if (section is null) return Error.NotFound("Laptop promo");

        if (cmd.Request.ProductId is { } pid && pid != Guid.Empty)
        {
            Product? product = await _uow.Products.GetByIdAsync(pid, cancellationToken);
            if (product is null) return Error.NotFound("Product");
            section.ProductId  = product.Id;
            section.SlugId     = $"laptop-promo-{product.Slug}";
            section.Slug       = product.Slug;
            section.Title      = product.Name;
            section.Subtitle   = product.ShortDescription;
            section.ImageUrl   = product.ImageUrls.FirstOrDefault();
            section.ImageAlt   = product.Name;
            section.PriceValue = PriceFormatter.Format(product.Price);
            section.PriceBadge = PriceFormatter.DiscountBadge(product.CompareAtPrice, product.Price) ?? "From";
        }
        if (cmd.Request.CtaText is not null) section.CtaText = cmd.Request.CtaText.Trim();

        section.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
