namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateProductEntryCommand(ProductEntryCommand Command) : IRequest<long>;

public class CreateProductEntryCommandHandler(
    IAppDbContext context,
    IFileStorageService fileStorage)
    : IRequestHandler<CreateProductEntryCommand, long>
{
    public async Task<long> Handle(CreateProductEntryCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var shop = await GetOrCreateDefaultShopAsync(cancellationToken);
            var defaultUnitMeasure = await GetOrCreateDefaultUnitMeasureAsync(cancellationToken);
            var defaultCurrency = await GetOrCreateDefaultCurrencyAsync(cancellationToken);

            var item = request.Command;
            
            var product = await GetOrCreateProductAsync(item, defaultUnitMeasure, cancellationToken);

            var productType = await GetOrCreateProductTypeAsync(item, product, defaultCurrency, cancellationToken);
            
            productType.BundleItemCount = item.BundleItemCount;
            productType.UnitPrice = item.UnitPrice;
            
            product.ProductionOrigin = item.ProductionOrigin;

            await TryDeductFromInProcessAsync(productType, item.Count, cancellationToken);

            var residue = await UpdateProductResidueAsync(productType, item.Count, shop, cancellationToken);

            var costPrice = CalculateCostPrice(productType);

            var entry = SaveProductEntry(item, productType, shop, residue, costPrice, defaultCurrency);

            await context.CommitTransactionAsync(cancellationToken);
            return entry.Id;
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
            context.Shops.Add(shop);
        }

        return shop;
    }

    private async Task<UnitMeasure> GetOrCreateDefaultUnitMeasureAsync(CancellationToken ct)
    {
        var unitMeasure = await context.UnitMeasures
            .FirstOrDefaultAsync(u => u.IsDefault || u.NormalizedName == "Dona".ToNormalized(), ct);

        if (unitMeasure is null)
        {
            unitMeasure = new UnitMeasure
            {
                Name = "Dona",
                NormalizedName = "DONA",
                Symbol = "dona",
                Description = "Default o'lchov birligi",
                IsDefault = true,
                IsActive = true,
                Position = 1
            };
            context.UnitMeasures.Add(unitMeasure);
        }

        return unitMeasure;
    }

    private async Task<Currency> GetOrCreateDefaultCurrencyAsync(CancellationToken ct)
    {
        var currency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Code == "UZS" || c.IsDefault, ct);

        if (currency is null)
        {
            currency = new Currency
            {
                Code = "UZS",
                Name = "So'm",
                NormalizedName = "So'm".ToNormalized(),
                Symbol = "so'm",
                ExchangeRate = 1,
                IsDefault = true,
                IsActive = true
            };
            context.Currencies.Add(currency);
        }

        return currency;
    }

    private async Task<Product> GetOrCreateProductAsync(ProductEntryCommand item, UnitMeasure defaultUnitMeasure, CancellationToken ct)
    {
        Product? product = null;

        if (item.Product.Id > 0)
        {
            product = await context.Products
                .Include(p => p.ProductTypes)
                .FirstOrDefaultAsync(p => p.Id == item.Product.Id, ct);
        }

        if (product is null && !string.IsNullOrWhiteSpace(item.Product.Code))
        {
            product = await context.Products
                .Include(p => p.ProductTypes)
                .FirstOrDefaultAsync(p => p.Code == item.Product.Code, ct);
        }

        if (product is null)
        {
            if (string.IsNullOrWhiteSpace(item.Product.Code) || string.IsNullOrWhiteSpace(item.Product.Name))
            {
                throw new AppException("Yangi mahsulot yaratish uchun Kod va Nom majburiy!");
            }

            var imagePath = item.Product.ImagePath;
            if (!string.IsNullOrWhiteSpace(imagePath) && imagePath.Contains("/temp/"))
            {
                var newKey = await fileStorage.MoveFileAsync(imagePath, "products", ct);
                if (newKey != null) imagePath = newKey;
            }

            product = new Product
            {
                Code = item.Product.Code,
                Name = item.Product.Name,
                NormalizedName = item.Product.Name.ToUpper(),
                ProductionOrigin = item.ProductionOrigin,
                UnitMeasure = defaultUnitMeasure,
                ImagePath = imagePath,
                ProductTypes = []
            };

            context.Products.Add(product);
        }

        return product;
    }

    private async Task<ProductType> GetOrCreateProductTypeAsync(
        ProductEntryCommand item,
        Product product,
        Currency defaultCurrency,
        CancellationToken ct)
    {
        ProductType? productType = null;

        var productTypeCommand = item.Product.ProductTypes.FirstOrDefault()
            ?? throw new NotFoundException("ProductType ma'lumotlari topilmadi!");

        if (productTypeCommand.Id > 0)
        {
            productType = await context.ProductTypes
                .Include(pt => pt.Product)
                .Include(pt => pt.ProductResidue)
                .Include(pt => pt.ProductTypeItems)
                    .ThenInclude(ti => ti.SemiProduct)
                        .ThenInclude(sp => sp!.SemiProductEntries)
                .FirstOrDefaultAsync(pt => pt.Id == productTypeCommand.Id && pt.ProductId == product.Id, ct);
        }

        if (productType is null && !string.IsNullOrWhiteSpace(productTypeCommand.Type))
        {
            productType = await context.ProductTypes
                .Include(pt => pt.Product)
                .Include(pt => pt.ProductResidue)
                .Include(pt => pt.ProductTypeItems)
                    .ThenInclude(ti => ti.SemiProduct)
                        .ThenInclude(sp => sp!.SemiProductEntries)
                .FirstOrDefaultAsync(pt => pt.Type == productTypeCommand.Type && pt.ProductId == product.Id, ct);
        }

        if (productType is null)
        {
            if (string.IsNullOrWhiteSpace(productTypeCommand.Type))
            {
                throw new AppException("Yangi ProductType yaratish uchun Type majburiy!");
            }

            productType = new ProductType
            {
                Type = productTypeCommand.Type,
                BundleItemCount = item.BundleItemCount,
                ProductId = product.Id,
                Product = product,
                ProductTypeItems = [],
                UnitPrice = item.UnitPrice,
                Currency = defaultCurrency
            };

            context.ProductTypes.Add(productType);

            product.ProductTypes ??= [];
            product.ProductTypes.Add(productType);
        }

        return productType;
    }

    private async Task TryDeductFromInProcessAsync(ProductType productType, int totalCount, CancellationToken ct)
    {
        if (productType.Id <= 0)
            return;

        var inProcess = await context.InProcesses
            .FirstOrDefaultAsync(p => p.ProductTypeId == productType.Id, ct);

        if (inProcess is not null && inProcess.Count >= totalCount)
        {
            inProcess.Count -= totalCount;
        }
    }

    private async Task<ProductResidue> UpdateProductResidueAsync(
        ProductType productType,
        int count,
        Shop shop,
        CancellationToken ct)
    {
        ProductResidue? residue = null;

        if (productType.Id > 0)
        {
            residue = await context.ProductResidues
                .FirstOrDefaultAsync(r => r.ProductTypeId == productType.Id && r.ShopId == shop.Id, ct);
        }

        if (residue is null)
        {
            residue = new ProductResidue
            {
                ProductType = productType,
                Shop = shop,
                Count = count,
                ProductEntries = []
            };
            context.ProductResidues.Add(residue);

            productType.ProductResidue = residue;
        }
        else
        {
            residue.Count += count;
        }

        return residue;
    }

    private static decimal CalculateCostPrice(ProductType productType)
    {
        if (productType.ProductTypeItems is null || !productType.ProductTypeItems.Any())
            return 0;

        decimal totalCostPrice = 0;

        foreach (var typeItem in productType.ProductTypeItems)
        {
            if (typeItem.SemiProduct?.SemiProductEntries is null ||
                !typeItem.SemiProduct.SemiProductEntries.Any())
                continue;

            var lastEntry = typeItem.SemiProduct.SemiProductEntries
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefault();

            if (lastEntry is not null)
            {
                totalCostPrice += lastEntry.CostPrice * typeItem.Quantity;
            }
        }

        return totalCostPrice;
    }

    private ProductEntry SaveProductEntry(
        ProductEntryCommand item,
        ProductType productType,
        Shop shop,
        ProductResidue residue,
        decimal costPrice,
        Currency defaultCurrency)
    {
        var totalAmount = item.Count * item.UnitPrice;

        var entry = new ProductEntry
        {
            Date = item.Date.ToUniversalTime(),
            Count = item.Count,
            BundleItemCount = item.BundleItemCount,
            CostPrice = costPrice,
            PreparationCostPerUnit = item.PreparationCostPerUnit,
            UnitPrice = item.UnitPrice,
            TotalAmount = totalAmount,
            ProductionOrigin = item.ProductionOrigin,
            ProductType = productType,
            Shop = shop,
            ProductResidue = residue,
            Currency = defaultCurrency
        };

        context.ProductEntries.Add(entry);

        residue.ProductEntries ??= [];
        residue.ProductEntries.Add(entry);

        productType.ProductEntries ??= [];
        productType.ProductEntries.Add(entry);

        return entry;
    }
}
