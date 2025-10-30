namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.SemiProducts;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateProductEntryCommand(List<ProductEntryCommand> Command) : IRequest<long>;

public class CreateProductEntryCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateProductEntryCommand, long>
{
    public async Task<long> Handle(CreateProductEntryCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var shop = await GetOrCreateDefaultShopAsync(cancellationToken);

            foreach (var item in request.Command)
            {
                var employee = await GetEmployeeAsync(item.EmployeeId, cancellationToken);
                await CreditEmployeeAsync(employee, item.TotalAmount, cancellationToken);

                var productType = await GetProductTypeAsync(item.ProductTypeId, cancellationToken);
                var totalCount = item.BundleCount * productType.Count;

                var residue = await UpdateProductResidueAsync(productType, item.BundleCount, shop.Id, cancellationToken);
                await DeductSemiProductResiduesAsync(productType, totalCount, cancellationToken);

                var costPrice = await CalculateCostPriceAsync(productType, totalCount, item.PreparationCostPerUnit, cancellationToken);
                await SaveProductEntryAsync(item, productType.Count, costPrice, shop, employee, residue, cancellationToken);
            }

            await context.CommitTransactionAsync(cancellationToken);
            return 1;
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Shop> GetOrCreateDefaultShopAsync(CancellationToken ct)
    {
        var shop = await context.Shops.FirstOrDefaultAsync(s => s.Id == 1, ct);

        if (shop is null)
        {
            shop = new Shop
            {
                Id = 1,
                Name = "Default Shop",
            };

            await context.Shops.AddAsync(shop, ct);
        }

        return shop;
    }

    private async Task<User> GetEmployeeAsync(long employeeId, CancellationToken ct)
    {
        var employee = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == employeeId, ct);

        return employee ?? throw new InvalidOperationException($"Employee not found: Id={employeeId}");
    }

    private async Task CreditEmployeeAsync(User employee, decimal amount, CancellationToken ct)
    {
        const long somCurrencyId = 1;

        var account = employee.Accounts
            .FirstOrDefault();

        if (account is null)
        {
            account = new UserAccount
            {
                UserId = employee.Id,
                CurrencyId = somCurrencyId,
                Balance = amount,
                Discount = 0,
                OpeningBalance = amount
            };

            await context.UserAccounts.AddAsync(account, ct);
        }
        else
        {
            account.Balance += amount;
        }
    }

    private async Task<ProductType> GetProductTypeAsync(long productTypeId, CancellationToken ct)
    {
        var productType = await context.ProductTypes
            .Include(pt => pt.ProductResidue)
            .Include(pt => pt.ProductTypeItems)
                .ThenInclude(i => i.SemiProduct)
            .FirstOrDefaultAsync(pt => pt.Id == productTypeId, ct);

        return productType ?? throw new InvalidOperationException($"ProductType not found: Id={productTypeId}");
    }

    private async Task<ProductResidue> UpdateProductResidueAsync(ProductType productType, int count, long shopId, CancellationToken ct)
    {
        var residue = await context.ProductResidues
            .FirstOrDefaultAsync(r => r.ProductTypeId == productType.Id && r.ShopId == shopId, ct);

        if (residue is null)
        {
            residue = new ProductResidue
            {
                ProductTypeId = productType.Id,
                ShopId = shopId,
                Count = count
            };

            productType.ProductResidue = residue;
            await context.ProductResidues.AddAsync(residue, ct);
        }
        else
        {
            residue.Count += count;
        }

        return residue;
    }

    private async Task DeductSemiProductResiduesAsync(ProductType productType, int countByType, CancellationToken ct)
    {
        foreach (var item in productType.ProductTypeItems)
        {
            var semiResidue = await context.SemiProductResidues
                .FirstOrDefaultAsync(r => r.SemiProductId == item.SemiProductId, ct);

            if (semiResidue is null)
            {
                semiResidue = new SemiProductResidue
                {
                    SemiProductId = item.SemiProductId,
                    ManufactoryId = 1,
                    Quantity = 0
                };

                await context.SemiProductResidues.AddAsync(semiResidue, ct);
            }

            semiResidue.Quantity -= item.Quantity * countByType;
        }
    }

    private async Task<decimal> CalculateCostPriceAsync(ProductType productType, int countByType, decimal preparationCost, CancellationToken ct)
    {
        decimal semiTotal = 0;

        foreach (var item in productType.ProductTypeItems)
        {
            var semiEntry = await context.SemiProductEntries
                .Where(e => e.SemiProductId == item.SemiProductId)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (semiEntry is not null)
            {
                var unitCost = semiEntry.CostPrice + semiEntry.CostDelivery + semiEntry.TransferFee;
                semiTotal += unitCost * item.Quantity * countByType;
            }
        }

        return semiTotal + preparationCost;
    }

    private async Task SaveProductEntryAsync(
        ProductEntryCommand item,
        int countByType,
        decimal costPrice,
        Shop shop,
        User employee,
        ProductResidue residue,
        CancellationToken ct)
    {
        var entry = new ProductEntry
        {
            BundleCount = item.BundleCount,
            BundleItemCount = countByType,
            PreparationCostPerUnit = item.PreparationCostPerUnit,
            CostPrice = costPrice,
            ProductTypeId = item.ProductTypeId,
            Shop = shop,
            Employee = employee,
            UnitPrice = costPrice / (countByType * item.BundleCount) + item.PreparationCostPerUnit,
            TotalAmount = item.TotalAmount,
            ProductResidue = residue
        };

        await context.ProductEntries.AddAsync(entry, ct);
    }
}
