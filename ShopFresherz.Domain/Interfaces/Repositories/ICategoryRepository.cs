using ShopFresherz.Domain.Entities;

namespace ShopFresherz.Domain.Interfaces.Repositories;

/// <summary>Repository contract for Category tree persistence operations.</summary>
public interface ICategoryRepository
{
    /// <summary>Retrieves a category by its integer ID.</summary>
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a category by its URL slug.</summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Returns the full category tree including children.</summary>
    Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all top-level categories (ParentId is null).</summary>
    Task<IReadOnlyList<Category>> GetTopLevelAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a new category.</summary>
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>Marks a category as modified.</summary>
    void Update(Category category);
}
