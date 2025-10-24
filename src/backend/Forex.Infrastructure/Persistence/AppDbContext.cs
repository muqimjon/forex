namespace Forex.Infrastructure.Persistence;

using Forex.Application.Commons.Interfaces;
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
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Manufactory> Manufactories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<ProductEntry> ProductEntries { get; set; }
    public DbSet<ProductTypeItem> ProductTypeItems { get; set; }
    public DbSet<SemiProductResidue> SemiProductResidues { get; set; }
    public DbSet<ProductResidue> ResidueShops { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<SemiProduct> SemiProducts { get; set; }
    public DbSet<SemiProductEntry> SemiProductEntries { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ProductResidue> ProductResidues { get; set; }
    public DbSet<UnitMeasure> UnitMeasures { get; set; }

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

        modelBuilder.Ignore<System.Transactions.Transaction>();
    }
}
