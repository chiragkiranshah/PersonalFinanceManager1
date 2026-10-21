using PersonalFinanceManager.Categories.Domain;

namespace PersonalFinanceManager.Categories.Application;

public record CreateCategoryRequest(string Name, string Type, bool IsSystemDefined = false);
public record UpdateCategoryRequest(string Name, string Type);
public record CategoryResponse(Guid Id, Guid UserId, string Name, string Type, bool IsSystemDefined, bool IsDeleted, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class CategoryService(ICategoryRepository repository)
{
    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<CategoryType>(request.Type, true, out var type))
        {
            throw new ArgumentException("Invalid category type.", nameof(request));
        }

        var category = Category.Create(userId, request.Name, type, request.IsSystemDefined);
        await repository.AddAsync(category, cancellationToken);
        return Map(category);
    }

    public async Task<CategoryResponse?> GetAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetByIdAsync(id, cancellationToken);
        if (category is null || !category.IsOwnedBy(userId)) return null;
        return Map(category);
    }

    public async Task<IReadOnlyList<CategoryResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = await repository.ListAsync(userId, cancellationToken);
        return categories.Select(Map).ToList();
    }

    public async Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetByIdAsync(id, cancellationToken);
        if (category is null || !category.IsOwnedBy(userId)) return null;

        if (!Enum.TryParse<CategoryType>(request.Type, true, out var type))
        {
            throw new ArgumentException("Invalid category type.", nameof(request));
        }

        category.Update(request.Name, type);
        await repository.UpdateAsync(category, cancellationToken);
        return Map(category);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetByIdAsync(id, cancellationToken);
        if (category is null || !category.IsOwnedBy(userId)) return false;

        category.Delete();
        await repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static CategoryResponse Map(Category category) => new(
        category.Id,
        category.UserId,
        category.Name,
        category.Type.ToString(),
        category.IsSystemDefined,
        category.IsDeleted,
        category.CreatedAtUtc,
        category.UpdatedAtUtc);
}
