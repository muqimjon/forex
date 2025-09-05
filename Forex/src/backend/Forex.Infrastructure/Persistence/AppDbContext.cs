namespace Forex.Infrastructure.Persistence;

using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading;
using System.Threading.Tasks;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<ShopCash> ShopCashes { get; set; }
    public DbSet<ContainerEntry> ContainerEntries { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Manufactory> Manufactories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductEntry> ProductEntries { get; set; }
    public DbSet<ProductItem> ProductItems { get; set; }
    public DbSet<ResidueManufactory> ResidueManufactories { get; set; }
    public DbSet<ResidueShop> ResidueShops { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<SemiProduct> SemiProducts { get; set; }
    public DbSet<SemiProductEntry> SemiProductEntries { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

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
}
