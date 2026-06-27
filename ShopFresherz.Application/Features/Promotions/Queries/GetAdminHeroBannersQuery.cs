using MediatR;
using ShopFresherz.Application.Common;
using ShopFresherz.Application.Dtos.Promotions;
using ShopFresherz.Domain.Entities;
using ShopFresherz.Domain.Interfaces;

namespace ShopFresherz.Application.Features.Promotions.Queries;

/// <summary>Returns all non-deleted hero banners for admin management.</summary>
public sealed record GetAdminHeroBannersQuery
    : IRequest<Result<IReadOnlyList<AdminHeroBannerDto>>>;

/// <summary>Handler for <see cref="GetAdminHeroBannersQuery"/>.</summary>
public sealed class GetAdminHeroBannersQueryHandler
    : IRequestHandler<GetAdminHeroBannersQuery, Result<IReadOnlyList<AdminHeroBannerDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetAdminHeroBannersQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<AdminHeroBannerDto>>> Handle(
        GetAdminHeroBannersQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<PromotionalSection> sections = await _uow.PromotionalSections
            .GetAllBySectionKeyAsync("hero", cancellationToken);

        IReadOnlyList<AdminHeroBannerDto> result = sections
            .Select(section => new AdminHeroBannerDto(
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
                section.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<AdminHeroBannerDto>>.Success(result);
    }
}
