using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Transactions.Domain;

namespace PersonalFinanceManager.Transactions.Infrastructure;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
    {
    }

    public DbSet<TransactionEntry> Transactions => Set<TransactionEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionEntry>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(x => x.Type).HasConversion<string>();
            entity.Property(x => x.OccurredOnUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.AccountId);
            entity.HasIndex(x => x.OccurredOnUtc);
            entity.HasIndex(x => x.DestinationAccountId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
