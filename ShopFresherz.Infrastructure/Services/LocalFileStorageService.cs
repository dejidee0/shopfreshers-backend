using Microsoft.Extensions.Hosting;
using ShopFresherz.Domain.Interfaces.Services;

namespace ShopFresherz.Infrastructure.Services;

/// <summary>Development file storage that writes public files under wwwroot/uploads.</summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsRoot;

    public LocalFileStorageService(IHostEnvironment environment)
    {
        _uploadsRoot = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads");
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        string safeRelativePath = fileName.Replace('\\', '/').TrimStart('/');
        string fullPath = Path.Combine(_uploadsRoot, safeRelativePath.Replace('/', Path.DirectorySeparatorChar));
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using FileStream fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return $"/uploads/{safeRelativePath}";
    }

    /// <inheritdoc />
    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        string relative = fileUrl.Replace('\\', '/');
        if (relative.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            relative = relative["/uploads/".Length..];
        }

        string fullPath = Path.Combine(_uploadsRoot, relative.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        string safeRelativePath = fileName.Replace('\\', '/').TrimStart('/');
        string fullPath = Path.Combine(_uploadsRoot, safeRelativePath.Replace('/', Path.DirectorySeparatorChar));
        return Task.FromResult(File.Exists(fullPath));
    }
}
