using MediatR;
using ShopFresherz.Application.Common;
using ShopFresherz.Application.Dtos.Product;
using ShopFresherz.Application.Features.Search.Commands;
using ShopFresherz.Domain.Entities;
using ShopFresherz.Domain.Interfaces;
using ShopFresherz.Domain.Interfaces.Services;

namespace ShopFresherz.Application.Features.Products.Commands;

/// <summary>Command for updating an existing product (admin only).</summary>
/// <param name="Id">The product ID to update.</param>
/// <param name="Request">The partial update payload.</param>
public sealed record UpdateProductCommand(Guid Id, UpdateProductRequest Request) : IRequest<Result<bool>>;

/// <summary>Handler for <see cref="UpdateProductCommand"/>.</summary>
public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ISearchService _search;

    /// <summary>Initialises the handler.</summary>
    public UpdateProductCommandHandler(IUnitOfWork uow, ISearchService search)
    {
        _uow = uow;
        _search = search;
    }

    /// <inheritdoc />
    public async Task<Result<bool>> Handle(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await _uow.Products.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return Error.NotFound("Product");
        }

        UpdateProductRequest req = command.Request;

        if (req.Name         is not null) product.Name             = req.Name.Trim();
        if (req.BrandId      is not null) product.BrandId          = req.BrandId.Value;
        if (req.CategoryId   is not null) product.CategoryId       = req.CategoryId.Value;
        if (req.Description  is not null) product.Description      = req.Description;
        if (req.ShortDescription is not null) product.ShortDescription = req.ShortDescription;
        if (req.Price        is not null) product.Price            = req.Price.Value;
        if (req.CompareAtPrice is not null) product.CompareAtPrice = req.CompareAtPrice;
        if (req.CostPrice    is not null) product.CostPrice        = req.CostPrice;
        if (req.StockQty     is not null) product.StockQty         = req.StockQty.Value;
        if (req.WeightKg     is not null) product.WeightKg         = req.WeightKg;
        if (req.AttributesJson is not null) product.AttributesJson = req.AttributesJson;
        if (req.TagsJson     is not null) product.TagsJson         = req.TagsJson;
        if (req.IsActive     is not null) product.IsActive         = req.IsActive.Value;
        if (req.IsFeatured   is not null) product.IsFeatured       = req.IsFeatured.Value;
        if (req.MetaTitle    is not null) product.MetaTitle        = req.MetaTitle;
        if (req.MetaDescription is not null) product.MetaDescription = req.MetaDescription;

        if (req.Slug is not null)
        {
            string newSlug = req.Slug.Trim().ToLowerInvariant();
            if (!string.Equals(newSlug, product.Slug, StringComparison.Ordinal)
                && await _uow.Products.SlugExistsAsync(newSlug, cancellationToken))
            {
                return Error.Conflict($"Slug '{newSlug}' is already taken.");
            }
            product.Slug = newSlug;
        }

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(cancellationToken);

        ProductSearchDocument doc = IndexProductCommandHandler.Map(product);
        _ = Task.Run(() => _search.IndexProductAsync(doc, CancellationToken.None), CancellationToken.None);

        return Result<bool>.Success(true);
    }
}
