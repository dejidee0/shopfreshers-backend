using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopFresherz.Application.Common;
using ShopFresherz.Application.Dtos.Product;
using ShopFresherz.Application.Features.Categories.Queries;

namespace ShopFresherz.API.Controllers;

/// <summary>Returns the product category tree for navigation and filtering.</summary>
[ApiController]
[Route("api/v1/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Initialises a new instance of <see cref="CategoriesController"/>.</summary>
    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns the full active category tree including children.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<CategoryDto>> result =
            await _mediator.Send(new GetCategoriesQuery(), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : StatusCode(500);
    }
}
