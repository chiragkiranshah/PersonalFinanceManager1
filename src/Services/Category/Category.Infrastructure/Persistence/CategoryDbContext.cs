using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Categories.Domain;

namespace PersonalFinanceManager.Categories.Infrastructure;

public class CategoryDbContext : DbContext
{
    public CategoryDbContext(DbContextOptions<CategoryDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>();
            entity.Property(x => x.IsSystemDefined).IsRequired();
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Type);
        });

        base.OnModelCreating(modelBuilder);
    }
}
