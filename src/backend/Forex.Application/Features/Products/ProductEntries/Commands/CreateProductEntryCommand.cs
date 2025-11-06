namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
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
                var productType = await GetProductTypeAsync(item.ProductTypeId, cancellationToken);
                var totalCount = item.BundleCount * productType.Count;

                await DeductFromInProcessAsync(productType.Id, totalCount, cancellationToken);
                var residue = await UpdateProductResidueAsync(productType.Id, item.BundleCount, shop.Id, cancellationToken);
                await SaveProductEntryAsync(item, productType.Count, shop, residue, cancellationToken);
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
            shop = new Shop { Id = 1, Name = "Default Shop" };
            await context.Shops.AddAsync(shop, ct);
        }

        return shop;
    }

    private async Task<ProductType> GetProductTypeAsync(long productTypeId, CancellationToken ct)
    {
        var productType = await context.ProductTypes
            .Include(pt => pt.ProductResidue)
            .FirstOrDefaultAsync(pt => pt.Id == productTypeId, ct);

        return productType ?? throw new InvalidOperationException($"ProductType not found. Id={productTypeId}");
    }

    private async Task DeductFromInProcessAsync(long productTypeId, int totalCount, CancellationToken ct)
    {
        var inProcess = await context.InProcesses.FirstOrDefaultAsync(p => p.ProductTypeId == productTypeId, ct);

        if (inProcess is null || inProcess.Quantity < totalCount)
            throw new InvalidOperationException($"Not enough in-process quantity. ProductTypeId={productTypeId}");

        inProcess.Quantity -= totalCount;
    }

    private async Task<ProductResidue> UpdateProductResidueAsync(long productTypeId, int bundleCount, long shopId, CancellationToken ct)
    {
        var residue = await context.ProductResidues
            .FirstOrDefaultAsync(r => r.ProductTypeId == productTypeId && r.ShopId == shopId, ct);

        if (residue is null)
        {
            residue = new ProductResidue
            {
                ProductTypeId = productTypeId,
                ShopId = shopId,
                Count = bundleCount
            };

            await context.ProductResidues.AddAsync(residue, ct);
        }
        else
        {
            residue.Count += bundleCount;
        }

        return residue;
    }

    private async Task SaveProductEntryAsync(
        ProductEntryCommand item,
        int countByType,
        Shop shop,
        ProductResidue residue,
        CancellationToken ct)
    {
        var entry = new ProductEntry
        {
            BundleCount = item.BundleCount,
            BundleItemCount = countByType,
            PreparationCostPerUnit = item.PreparationCostPerUnit,
            ProductTypeId = item.ProductTypeId,
            Shop = shop,
            UnitPrice = 0,
            TotalAmount = item.TotalAmount,
            ProductResidue = residue
        };

        await context.ProductEntries.AddAsync(entry, ct);
    }
}
