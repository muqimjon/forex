namespace Forex.Application.Commons.Interfaces;

using Forex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Account> Accounts { get; }
    DbSet<ShopCash> ShopCashes { get; }
    DbSet<ContainerEntry> ContainerEntries { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Manufactory> Manufactories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductEntry> ProductEntries { get; }
    DbSet<ProductItem> ProductItems { get; }
    DbSet<ResidueManufactory> ResidueManufactories { get; }
    DbSet<ResidueShop> ResidueShops { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItem> SaleItems { get; }
    DbSet<SemiProduct> SemiProducts { get; }
    DbSet<SemiProductEntry> SemiProductEntries { get; }
    DbSet<Shop> Shops { get; }
    DbSet<Transaction> Transactions { get; }

    Task<bool> SaveAsync(CancellationToken cancellation);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<bool> CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
