namespace Forex.Application.Features.Products.Products.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateProductCommand : IRequest<long>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long UnitMeasureId { get; set; }
    public string? ImagePath { get; set; }
    public Domain.Enums.ProductionOrigin ProductionOrigin { get; set; }
    public ICollection<ProductTypes.Commands.ProductTypeCommand> ProductTypes { get; set; } = [];
}

public class CreateProductCommandHandler(
    IAppDbContext context,
    IFileStorageService fileStorage)
    : IRequestHandler<CreateProductCommand, long>
{
    public async Task<long> Handle(CreateProductCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new AppException("Mahsulot nomi kiritilishi shart!");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new AppException("Mahsulot kodi kiritilishi shart!");

        var codeExists = await context.Products
            .AnyAsync(p => p.Code == request.Code && !p.IsDeleted, ct);

        if (codeExists)
            throw new AppException($"'{request.Code}' kodli mahsulot allaqachon mavjud!");

        await context.BeginTransactionAsync(ct);

        try
        {
            // Image: temp dan asosiy joyga ko'chirish
            var imagePath = fileStorage.IsTempKey(request.ImagePath)
                ? await fileStorage.MoveFileAsync(request.ImagePath!, "products", ct)
                : null;

            var unitMeasure = await context.UnitMeasures
                .FirstOrDefaultAsync(u => u.Id == request.UnitMeasureId, ct)
                ?? throw new NotFoundException("UnitMeasure", "Id", request.UnitMeasureId);

            var product = new Product
            {
                Code = request.Code,
                Name = request.Name,
                NormalizedName = request.Name.ToNormalized(),
                ImagePath = imagePath,
                ProductionOrigin = request.ProductionOrigin,
                UnitMeasureId = unitMeasure.Id,
                UnitMeasure = unitMeasure,
                ProductTypes = []
            };

            context.Products.Add(product);

            // Default valyuta
            var defaultCurrency = await context.Currencies
                .FirstOrDefaultAsync(c => c.IsDefault || c.Code == "UZS", ct)
                ?? throw new NotFoundException("Currency", "IsDefault", true);

            // ProductType'lar yaratish
            foreach (var typeCmd in request.ProductTypes)
            {
                if (string.IsNullOrWhiteSpace(typeCmd.Type))
                    continue;

                // ✅ VALIDATSIYA: ProductType qiymatlari
                if (typeCmd.BundleItemCount <= 0)
                    throw new AppException($"'{typeCmd.Type}' turi uchun qopdagi dona soni (BundleItemCount) 0 dan katta bo'lishi shart!");

                if (typeCmd.UnitPrice < 0)
                    throw new AppException($"'{typeCmd.Type}' turi uchun birlik narxi (UnitPrice) manfiy bo'lishi mumkin emas!");

                var productType = new ProductType
                {
                    Type = typeCmd.Type,
                    BundleItemCount = typeCmd.BundleItemCount,
                    PackItemCount = typeCmd.PackItemCount,
                    UnitPrice = typeCmd.UnitPrice,
                    Product = product,
                    Currency = defaultCurrency,
                    ProductEntries = []
                };

                context.ProductTypes.Add(productType);
                product.ProductTypes.Add(productType);
            }

            await context.SaveAsync(ct);

            foreach (var productType in product.ProductTypes)
                BarcodeGenerator.EnsureBarcodes(productType);

            await context.CommitTransactionAsync(ct);
            return product.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
