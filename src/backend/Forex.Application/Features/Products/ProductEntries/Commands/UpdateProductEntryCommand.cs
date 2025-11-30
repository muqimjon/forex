namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Products.Products.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateProductEntryCommand(
    long Id,
    DateTime Date,
    int Count,
    int BundleItemCount,
    decimal PreparationCostPerUnit,
    decimal UnitPrice,
    ProductionOrigin ProductionOrigin,
    ProductCommand Product)
    : IRequest<long>;

public class UpdateProductEntryCommandHandler(
    IAppDbContext context)
    : IRequestHandler<UpdateProductEntryCommand, long>
{
    public async Task<long> Handle(UpdateProductEntryCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            // Mavjud entry ni topish
            var existingEntry = await context.ProductEntries
                .Include(e => e.ProductType)
                    .ThenInclude(pt => pt.Product)
                .Include(e => e.ProductType)
                    .ThenInclude(pt => pt.ProductResidue)
                .Include(e => e.ProductType)
                    .ThenInclude(pt => pt.ProductTypeItems)
                        .ThenInclude(ti => ti.SemiProduct)
                            .ThenInclude(sp => sp!.SemiProductEntries)
                .Include(e => e.ProductResidue)
                .Include(e => e.Shop)
                .Include(e => e.Currency)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                ?? throw new AppException("ProductEntry topilmadi!");

            // Default qiymatlarni olish
            var defaultUnitMeasure = await GetOrCreateDefaultUnitMeasureAsync(cancellationToken);
            var defaultCurrency = await GetOrCreateDefaultCurrencyAsync(cancellationToken);

            // Eski ProductType va count ni saqlash
            var oldProductTypeId = existingEntry.ProductTypeId;
            var oldCount = existingEntry.Count;
            var oldProductResidue = existingEntry.ProductResidue;

            // Yangi Product va ProductType ni olish/yaratish
            var product = await GetOrCreateProductAsync(request, defaultUnitMeasure, cancellationToken);
            var productType = await GetOrCreateProductTypeAsync(request, product, defaultCurrency, cancellationToken);

            // ProductType va Product ma'lumotlarini yangilash
            productType.BundleItemCount = request.BundleItemCount;
            productType.UnitPrice = request.UnitPrice;
            product.ProductionOrigin = request.ProductionOrigin;

            // Agar ProductType o'zgargan bo'lsa
            bool productTypeChanged = oldProductTypeId != productType.Id;

            if (productTypeChanged)
            {
                // 1. Eski ProductResidue dan eski count ni ayirish
                if (oldProductResidue is not null)
                {
                    oldProductResidue.Count -= oldCount;
                }

                // 2. Eski InProcess ga eski count ni qaytarish
                await AddToOldInProcessAsync(oldProductTypeId, oldCount, cancellationToken);

                // 3. Yangi InProcess dan yangi count ni ayirish
                await TryDeductFromInProcessAsync(productType, request.Count, cancellationToken);

                // 4. Yangi ProductResidue ni topish yoki yaratish
                var newResidue = await GetOrCreateProductResidueAsync(
                    productType,
                    request.Count,
                    existingEntry.Shop,
                    cancellationToken);

                // Entry ni yangi ProductResidue ga bog'lash
                existingEntry.ProductResidue = newResidue;
                existingEntry.ProductResidueId = newResidue.Id;
            }
            else
            {
                // Agar ProductType o'zgarmagan bo'lsa, faqat count farqini hisoblash
                int countDifference = request.Count - oldCount;

                if (countDifference != 0)
                {
                    // ProductResidue ni yangilash
                    if (oldProductResidue is not null)
                    {
                        oldProductResidue.Count += countDifference;
                    }

                    // InProcess ni yangilash
                    if (countDifference > 0)
                    {
                        // Count oshgan bo'lsa, InProcess dan ayirish
                        await TryDeductFromInProcessAsync(productType, countDifference, cancellationToken);
                    }
                    else
                    {
                        // Count kamaygan bo'lsa, InProcess ga qaytarish
                        await AddToOldInProcessAsync(productType.Id, Math.Abs(countDifference), cancellationToken);
                    }
                }
            }

            // CostPrice ni hisoblash
            var costPrice = CalculateCostPrice(productType);

            // Entry ni yangilash
            UpdateEntry(existingEntry, request, productType, costPrice, defaultCurrency);

            await context.CommitTransactionAsync(cancellationToken);
            return existingEntry.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task AddToOldInProcessAsync(long productTypeId, int count, CancellationToken ct)
    {
        if (productTypeId <= 0)
            return;

        var inProcess = await context.InProcesses
            .FirstOrDefaultAsync(p => p.ProductTypeId == productTypeId, ct);

        if (inProcess is not null)
        {
            inProcess.Count += count;
        }
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

    private async Task<Product> GetOrCreateProductAsync(UpdateProductEntryCommand request, UnitMeasure defaultUnitMeasure, CancellationToken ct)
    {
        Product? product = null;

        // Agar Product.Id mavjud bo'lsa, ID bo'yicha qidirish
        if (request.Product.Id > 0)
        {
            product = await context.Products
                .Include(p => p.ProductTypes)
                .FirstOrDefaultAsync(p => p.Id == request.Product.Id, ct);
        }

        // Agar topilmasa, Code bo'yicha qidirish
        if (product is null && !string.IsNullOrWhiteSpace(request.Product.Code))
        {
            product = await context.Products
                .Include(p => p.ProductTypes)
                .FirstOrDefaultAsync(p => p.Code == request.Product.Code, ct);
        }

        // Agar hali ham topilmasa, yangi Product yaratish
        if (product is null)
        {
            if (string.IsNullOrWhiteSpace(request.Product.Code) || string.IsNullOrWhiteSpace(request.Product.Name))
            {
                throw new AppException("Yangi mahsulot yaratish uchun Kod va Nom majburiy!");
            }

            product = new Product
            {
                Code = request.Product.Code,
                Name = request.Product.Name,
                NormalizedName = request.Product.Name.ToUpper(),
                ProductionOrigin = request.ProductionOrigin,
                UnitMeasure = defaultUnitMeasure,
                ImagePath = request.Product.ImagePath,
                ProductTypes = []
            };

            context.Products.Add(product);
        }
        else
        {
            // Mavjud mahsulot ma'lumotlarini yangilash
            if (!string.IsNullOrWhiteSpace(request.Product.Name) && product.Name != request.Product.Name)
            {
                product.Name = request.Product.Name;
                product.NormalizedName = request.Product.Name.ToUpper();
            }

            product.ProductionOrigin = request.ProductionOrigin;

            // ImagePath ni yangilash (agar kiritilgan bo'lsa)
            if (!string.IsNullOrWhiteSpace(request.Product.ImagePath))
            {
                product.ImagePath = request.Product.ImagePath;
            }
        }

        return product;
    }

    private async Task<ProductType> GetOrCreateProductTypeAsync(
        UpdateProductEntryCommand request,
        Product product,
        Currency defaultCurrency,
        CancellationToken ct)
    {
        ProductType? productType = null;

        var productTypeRequest = request.Product.ProductTypes.FirstOrDefault()
            ?? throw new AppException("ProductType ma'lumotlari topilmadi!");

        // ProductTypeId bo'yicha qidirish
        if (productTypeRequest.Id > 0)
        {
            productType = await context.ProductTypes
                .Include(pt => pt.Product)
                .Include(pt => pt.ProductResidue)
                .Include(pt => pt.ProductTypeItems)
                    .ThenInclude(ti => ti.SemiProduct)
                        .ThenInclude(sp => sp!.SemiProductEntries)
                .FirstOrDefaultAsync(pt => pt.Id == productTypeRequest.Id && pt.ProductId == product.Id, ct);
        }

        // Agar topilmasa, Type bo'yicha qidirish
        if (productType is null && !string.IsNullOrWhiteSpace(productTypeRequest.Type))
        {
            productType = await context.ProductTypes
                .Include(pt => pt.Product)
                .Include(pt => pt.ProductResidue)
                .Include(pt => pt.ProductTypeItems)
                    .ThenInclude(ti => ti.SemiProduct)
                        .ThenInclude(sp => sp!.SemiProductEntries)
                .FirstOrDefaultAsync(pt => pt.Type == productTypeRequest.Type && pt.ProductId == product.Id, ct);
        }

        // Agar hali ham topilmasa, yangi ProductType yaratish
        if (productType is null)
        {
            if (string.IsNullOrWhiteSpace(productTypeRequest.Type))
            {
                throw new AppException("Yangi ProductType yaratish uchun Type majburiy!");
            }

            productType = new ProductType
            {
                Type = productTypeRequest.Type,
                BundleItemCount = request.BundleItemCount,
                ProductId = product.Id,
                Product = product,
                ProductTypeItems = [],
                UnitPrice = request.UnitPrice,
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
        // Faqat mavjud ProductType lar uchun InProcess dan ayirish
        if (productType.Id <= 0)
            return;

        var inProcess = await context.InProcesses
            .FirstOrDefaultAsync(p => p.ProductTypeId == productType.Id, ct);

        if (inProcess is not null && inProcess.Count >= totalCount)
        {
            inProcess.Count -= totalCount;
        }
    }

    private async Task<ProductResidue> GetOrCreateProductResidueAsync(
        ProductType productType,
        int count,
        Shop shop,
        CancellationToken ct)
    {
        ProductResidue? residue = null;

        // Agar ProductType ning ID si mavjud bo'lsa, ID orqali qidirish
        if (productType.Id > 0)
        {
            residue = await context.ProductResidues
                .FirstOrDefaultAsync(r => r.ProductTypeId == productType.Id && r.ShopId == shop.Id, ct);
        }

        // Agar topilmasa, yangi residue yaratish
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

            // ProductType ga ProductResidue ni bog'lash
            productType.ProductResidue = residue;
        }
        else
        {
            residue.Count += count;
        }

        return residue;
    }

    private decimal CalculateCostPrice(ProductType productType)
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

    private void UpdateEntry(
        ProductEntry existingEntry,
        UpdateProductEntryCommand request,
        ProductType productType,
        decimal costPrice,
        Currency defaultCurrency)
    {
        var totalAmount = request.Count * request.UnitPrice;

        // Mavjud entry ni yangilash (ID saqlanadi)
        existingEntry.Date = request.Date.ToUniversalTime();
        existingEntry.Count = request.Count;
        existingEntry.BundleItemCount = request.BundleItemCount;
        existingEntry.CostPrice = costPrice;
        existingEntry.PreparationCostPerUnit = request.PreparationCostPerUnit;
        existingEntry.UnitPrice = request.UnitPrice;
        existingEntry.TotalAmount = totalAmount;
        existingEntry.ProductionOrigin = request.ProductionOrigin;
        existingEntry.ProductType = productType;
        existingEntry.ProductTypeId = productType.Id;
        existingEntry.Currency = defaultCurrency;
        existingEntry.CurrencyId = defaultCurrency.Id;
    }
}