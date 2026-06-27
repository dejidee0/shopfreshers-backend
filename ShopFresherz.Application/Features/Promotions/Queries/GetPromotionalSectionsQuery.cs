using MediatR;
using ShopFresherz.Application.Common;
using ShopFresherz.Application.Dtos.Promotions;
using ShopFresherz.Domain.Interfaces;

namespace ShopFresherz.Application.Features.Promotions.Queries;

// ── Hero Banners ──────────────────────────────────────────────────────────────

/// <summary>Retrieves all active hero banner carousel slides.</summary>
public sealed record GetHeroBannersQuery : IRequest<Result<IReadOnlyList<HeroBannerDto>>>;

/// <summary>Handler for <see cref="GetHeroBannersQuery"/>.</summary>
public sealed class GetHeroBannersQueryHandler
    : IRequestHandler<GetHeroBannersQuery, Result<IReadOnlyList<HeroBannerDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetHeroBannersQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<HeroBannerDto>>> Handle(
        GetHeroBannersQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<HeroBannerDto> results = await _uow.PromotionalSections
            .GetBySectionKeyAsync("hero", cancellationToken,
                p => new HeroBannerDto(
                    p.SlugId, p.Tag, p.Badge, p.Title,
                    p.PriceText, p.CtaText, p.ImageUrl, p.SortOrder, p.Slug));

        return Result<IReadOnlyList<HeroBannerDto>>.Success(results);
    }
}

// ── Best Deal Card (single) ───────────────────────────────────────────────────

/// <summary>Retrieves the single active "Best Deal" featured product card.</summary>
public sealed record GetBestDealCardQuery : IRequest<Result<BestDealCardDto?>>;

/// <summary>Handler for <see cref="GetBestDealCardQuery"/>.</summary>
public sealed class GetBestDealCardQueryHandler
    : IRequestHandler<GetBestDealCardQuery, Result<BestDealCardDto?>>
{
    private readonly IUnitOfWork _uow;

    public GetBestDealCardQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<BestDealCardDto?>> Handle(
        GetBestDealCardQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<BestDealCardDto> results = await _uow.PromotionalSections
            .GetBySectionKeyAsync("best-deal", cancellationToken,
                p => new BestDealCardDto(
                    p.SlugId, p.ImageUrl, p.Title, p.Rating,
                    p.OriginalPriceText, p.SalePriceText, p.Description, p.Badge, p.Slug));

        return Result<BestDealCardDto?>.Success(results.FirstOrDefault());
    }
}

// ── Promo Banner (single) ─────────────────────────────────────────────────────

/// <summary>Retrieves the single active promotion section banner.</summary>
public sealed record GetPromoBannerQuery : IRequest<Result<PromoBannerDto?>>;

/// <summary>Handler for <see cref="GetPromoBannerQuery"/>.</summary>
public sealed class GetPromoBannerQueryHandler
    : IRequestHandler<GetPromoBannerQuery, Result<PromoBannerDto?>>
{
    private readonly IUnitOfWork _uow;

    public GetPromoBannerQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PromoBannerDto?>> Handle(
        GetPromoBannerQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<PromoBannerDto> results = await _uow.PromotionalSections
            .GetBySectionKeyAsync("promo-banner", cancellationToken,
                p => new PromoBannerDto(
                    p.SlugId, p.Title, p.Subtitle, p.CtaText,
                    p.ImageUrl, p.ImageAlt, p.Badge));

        return Result<PromoBannerDto?>.Success(results.FirstOrDefault());
    }
}

// ── Accessories Promo Card (single) ──────────────────────────────────────────

/// <summary>Retrieves the single active Computer Accessories featured product card.</summary>
public sealed record GetAccessoriesPromoQuery : IRequest<Result<FeatureCardDto?>>;

/// <summary>Handler for <see cref="GetAccessoriesPromoQuery"/>.</summary>
public sealed class GetAccessoriesPromoQueryHandler
    : IRequestHandler<GetAccessoriesPromoQuery, Result<FeatureCardDto?>>
{
    private readonly IUnitOfWork _uow;

    public GetAccessoriesPromoQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<FeatureCardDto?>> Handle(
        GetAccessoriesPromoQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<FeatureCardDto> results = await _uow.PromotionalSections
            .GetBySectionKeyAsync("accessories-promo", cancellationToken,
                p => new FeatureCardDto(
                    p.SlugId, p.ContentType, p.Title, p.Subtitle,
                    p.ButtonText, p.ImageUrl, p.PriceLabel, p.PriceValue, p.Slug));

        return Result<FeatureCardDto?>.Success(results.FirstOrDefault());
    }
}

// ── Laptop / Bottom Promo (single) ────────────────────────────────────────────

/// <summary>Retrieves the single active bottom promo card (below Computer Accessories section).</summary>
public sealed record GetLaptopPromoQuery : IRequest<Result<LaptopPromoDto?>>;

/// <summary>Handler for <see cref="GetLaptopPromoQuery"/>.</summary>
public sealed class GetLaptopPromoQueryHandler
    : IRequestHandler<GetLaptopPromoQuery, Result<LaptopPromoDto?>>
{
    private readonly IUnitOfWork _uow;

    public GetLaptopPromoQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<LaptopPromoDto?>> Handle(
        GetLaptopPromoQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<LaptopPromoDto> results = await _uow.PromotionalSections
            .GetBySectionKeyAsync("laptop-promo", cancellationToken,
                p => new LaptopPromoDto(
                    p.SlugId, p.Title, p.Subtitle, p.CtaText,
                    p.ImageUrl, p.ImageAlt, p.PriceBadge, p.PriceValue, p.Slug));

        return Result<LaptopPromoDto?>.Success(results.FirstOrDefault());
    }
}
