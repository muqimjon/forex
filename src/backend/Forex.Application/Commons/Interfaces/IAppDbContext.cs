namespace Forex.Application.Commons.Interfaces;

using Forex.Domain.Entities;
using Forex.Domain.Entities.Manufactories;
using Forex.Domain.Entities.Payments;
using Forex.Domain.Entities.Sales;
using Forex.Domain.Entities.Shops;
using Forex.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Account> Accounts { get; set; }
    DbSet<UserAccount> UserAccounts { get; set; }
    DbSet<ShopCashAccount> ShopCashAccounts { get; set; }
    DbSet<Currency> Currencies { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Manufactory> Manufactories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductResidue> ProductResidues { get; }
    DbSet<ProductType> ProductTypes { get; }
    DbSet<ProductEntry> ProductEntries { get; }
    DbSet<ProductTypeItem> ProductItems { get; }
    DbSet<SemiProductResidue> SemiProductResidues { get; }
    DbSet<ProductResidue> ResidueShops { get; }
    DbSet<UnitMeasure> UnitMeasures { get; }
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
