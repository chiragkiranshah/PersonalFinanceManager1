using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Accounts.Application;
using PersonalFinanceManager.Accounts.Domain;

namespace PersonalFinanceManager.Accounts.Infrastructure;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            entity.Property(x => x.OpeningBalance).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(x => x.CurrentBalance).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(x => x.Status).HasConversion<string>();
            entity.Property(x => x.Type).HasConversion<string>();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.Type);
        });

        base.OnModelCreating(modelBuilder);
    }
}
