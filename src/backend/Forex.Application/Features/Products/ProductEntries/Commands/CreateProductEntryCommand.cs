namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Products.Products.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateProductEntryCommand : IRequest<long>
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public int PackItemCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }
    public ProductCommand Product { get; set; } = default!;
}

public class CreateProductEntryCommandHandler(
    IAppDbContext context,
    IFileStorageService fileStorage)
    : IRequestHandler<CreateProductEntryCommand, long>
{
    public async Task<long> Handle(CreateProductEntryCommand request, CancellationToken cancellationToken)
    {
        // ✅ VALIDATSIYA: Majburiy qiymatlarni tekshirish
        if (request.Count <= 0)
            throw new AppException("Mahsulot miqdori (Count) 0 dan katta bo'lishi shart!");

        if (request.BundleItemCount <= 0)
            throw new AppException("Qopdagi dona soni (BundleItemCount) 0 dan katta bo'lishi shart!");

        if (request.UnitPrice <= 0)
            throw new AppException("Birlik narxi (UnitPrice) 0 dan katta bo'lishi shart!");

        if (request.PreparationCostPerUnit < 0)
            throw new AppException("Tayyorlanish narxi manfiy bo'lishi mumkin emas!");

        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var shop = await GetOrCreateDefaultShopAsync(cancellationToken);
            var defaultUnitMeasure = await GetOrCreateDefaultUnitMeasureAsync(cancellationToken);
            var defaultCurrency = await GetOrCreateDefaultCurrencyAsync(cancellationToken);

            var product = await GetOrCreateProductAsync(request, defaultUnitMeasure, cancellationToken);

            var productType = await GetOrCreateProductTypeAsync(request, product, defaultCurrency, cancellationToken);

            productType.BundleItemCount = request.BundleItemCount;
            if (request.PackItemCount > 0)
                productType.PackItemCount = request.PackItemCount;
            productType.UnitPrice = request.UnitPrice;

            product.ProductionOrigin = request.ProductionOrigin;

            var residue = await UpdateProductResidueAsync(productType, request.Count, shop, cancellationToken);

            var entry = SaveProductEntry(request, productType, shop, residue, defaultCurrency);

            await context.SaveAsync(cancellationToken);
            BarcodeGenerator.EnsureBarcodes(productType);

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

    private async Task<Product> GetOrCreateProductAsync(CreateProductEntryCommand item, UnitMeasure defaultUnitMeasure, CancellationToken ct)
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

            var imagePath = fileStorage.IsTempKey(item.Product.ImagePath)
                ? await fileStorage.MoveFileAsync(item.Product.ImagePath!, "products", ct)
                : null;

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
        CreateProductEntryCommand item,
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
                .FirstOrDefaultAsync(pt => pt.Id == productTypeCommand.Id && pt.ProductId == product.Id, ct);
        }

        if (productType is null && !string.IsNullOrWhiteSpace(productTypeCommand.Type))
        {
            productType = await context.ProductTypes
                .Include(pt => pt.Product)
                .Include(pt => pt.ProductResidue)
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
                UnitPrice = item.UnitPrice,
                Currency = defaultCurrency
            };

            context.ProductTypes.Add(productType);

            product.ProductTypes ??= [];
            product.ProductTypes.Add(productType);
        }

        return productType;
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

    private ProductEntry SaveProductEntry(
        CreateProductEntryCommand item,
        ProductType productType,
        Shop shop,
        ProductResidue residue,
        Currency defaultCurrency)
    {
        var totalAmount = (decimal)item.Count * item.UnitPrice;

        var entry = new ProductEntry
        {
            Date = item.Date.ToUniversalTime(),
            Count = item.Count,
            BundleItemCount = item.BundleItemCount,
            CostPrice = 0,
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
