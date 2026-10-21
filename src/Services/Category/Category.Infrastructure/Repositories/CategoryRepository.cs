using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Categories.Application;
using PersonalFinanceManager.Categories.Domain;

namespace PersonalFinanceManager.Categories.Infrastructure;

public class CategoryRepository(CategoryDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.Categories
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await context.Categories.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        context.Categories.Update(category);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null) return;
        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
    }
}
