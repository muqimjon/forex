namespace Forex.Infrastructure.Persistence;

using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.Sales;
using Forex.Domain.Entities.SemiProducts;
using Forex.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<ShopAccount> ShopCashAccounts { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<ProductEntry> ProductEntries { get; set; }
    public DbSet<ProductResidue> ResidueShops { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<Return> Returns { get; set; }
    public DbSet<ReturnItem> ReturnItems { get; set; }
    public DbSet<SemiProduct> SemiProducts { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ProductResidue> ProductResidues { get; set; }
    public DbSet<UnitMeasure> UnitMeasures { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }
    public DbSet<CompanyInfo> CompanyInfo { get; set; }
    public DbSet<SocialLink> SocialLinks { get; set; }
    public DbSet<OperationRecord> OperationRecords { get; set; }
    public DbSet<Supply> Supplies { get; set; }

    private IDbContextTransaction? currentTransaction;

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction is not null)
            return currentTransaction;

        currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        return currentTransaction;
    }

    public async Task<bool> CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        bool isSuccess;
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (currentTransaction is not null)
                await currentTransaction.CommitAsync(cancellationToken);
            isSuccess = true;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (currentTransaction is not null)
            {
                await currentTransaction.DisposeAsync();
                currentTransaction = null;
            }

        }

        return isSuccess;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction is not null)
        {
            await currentTransaction.RollbackAsync(cancellationToken);
            await currentTransaction.DisposeAsync();
            currentTransaction = null;
        }
    }

    public async Task<bool> SaveAsync(CancellationToken cancellation)
        => await SaveChangesAsync(cancellation) > 0;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>()
            .ToTable("Accounts")
            .HasDiscriminator<string>("AccountType")
            .HasValue<UserAccount>("Customer")
            .HasValue<ShopAccount>("ShopCash");

        modelBuilder.Entity<Supply>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Supply>()
            .HasOne(s => s.Currency)
            .WithMany()
            .HasForeignKey(s => s.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OperationRecord>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OperationRecord>()
            .HasOne(o => o.Currency)
            .WithMany()
            .HasForeignKey(o => o.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(u => u.SettlementCurrency)
            .WithMany()
            .HasForeignKey(u => u.SettlementCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Currency)
            .WithMany()
            .HasForeignKey(s => s.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Return>()
            .HasOne(r => r.Currency)
            .WithMany()
            .HasForeignKey(r => r.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Return>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Return>()
            .HasOne(r => r.OperationRecord)
            .WithOne(o => o.Return)
            .HasForeignKey<Return>(r => r.OperationRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Sale)
            .WithMany()
            .HasForeignKey(t => t.SaleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProductType>()
            .HasIndex(p => p.QopBarcode)
            .IsUnique()
            .HasFilter("\"QopBarcode\" IS NOT NULL AND \"QopBarcode\" <> ''");

        modelBuilder.Entity<ProductType>()
            .HasIndex(p => p.PackBarcode)
            .IsUnique()
            .HasFilter("\"PackBarcode\" IS NOT NULL AND \"PackBarcode\" <> ''");

        modelBuilder.Ignore<System.Transactions.Transaction>();
    }
}
