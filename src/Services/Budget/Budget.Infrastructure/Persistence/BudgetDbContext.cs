using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Budgets.Domain;

namespace PersonalFinanceManager.Budgets.Infrastructure;

public class BudgetDbContext : DbContext
{
    public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options)
    {
    }

    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("Budgets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(x => x.SpentAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(x => x.Period).HasConversion<string>();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.CategoryId);
            entity.HasIndex(x => x.StartDateUtc);
        });

        base.OnModelCreating(modelBuilder);
    }
}
