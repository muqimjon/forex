namespace Forex.Application.Features.Products.Products.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateProductCommand : IRequest<bool>
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long UnitMeasureId { get; set; }
    public string? ImagePath { get; set; }
    public Domain.Enums.ProductionOrigin ProductionOrigin { get; set; }
    public ICollection<ProductTypes.Commands.ProductTypeCommand> ProductTypes { get; set; } = [];
}

public class UpdateProductCommandHandler(
    IAppDbContext context,
    IFileStorageService fileStorage)
    : IRequestHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        // ✅ VALIDATSIYA: ID tekshirish
        if (request.Id <= 0)
            throw new AppException("Mahsulot ID si noto'g'ri!");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new AppException("Mahsulot nomi kiritilishi shart!");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new AppException("Mahsulot kodi kiritilishi shart!");

        var product = await context.Products
            .Include(p => p.ProductTypes)
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, ct);

        // DEBUG: Agar mahsulot topilmasa, aniq sabab yozish
        if (product is null)
        {
            var deletedProduct = await context.Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (deletedProduct is not null && deletedProduct.IsDeleted)
                throw new AppException($"ID: {request.Id} li mahsulot o'chirilgan!");

            throw new NotFoundException(nameof(Product), "Id", request.Id);
        }

        // Code uniqueness tekshirish (o'zidan boshqa)
        var codeExists = await context.Products
            .AnyAsync(p => p.Code == request.Code && p.Id != request.Id && !p.IsDeleted, ct);

        if (codeExists)
            throw new AppException($"'{request.Code}' kodli mahsulot allaqachon mavjud!");

        await context.BeginTransactionAsync(ct);

        try
        {
            // === IMAGE HANDLING ===
            var oldImagePath = product.ImagePath;
            var newImagePath = request.ImagePath;
            string? imagePathToDelete = null; // O'chiriladigan rasm yo'lini saqlaymiz

            if (fileStorage.IsTempKey(newImagePath))
            {
                // Yangi rasm yuklangan: temp dan ko'chirish
                var movedKey = await fileStorage.MoveFileAsync(newImagePath!, "products", ct);
                product.ImagePath = movedKey ?? newImagePath;

                // Eski rasmni KEYINROQ o'chirish uchun saqlab qo'yamiz
                if (!string.IsNullOrWhiteSpace(oldImagePath))
                    imagePathToDelete = oldImagePath;
            }
            else if (string.IsNullOrWhiteSpace(newImagePath) && !string.IsNullOrWhiteSpace(oldImagePath))
            {
                // Rasm o'chirilgan
                imagePathToDelete = oldImagePath;
                product.ImagePath = null;
            }
            // else: rasm o'zgarmagan — hech narsa qilinmaydi

            // === PRODUCT FIELDS ===
            product.Name = request.Name;
            product.NormalizedName = request.Name.ToNormalized();
            product.Code = request.Code;
            product.UnitMeasureId = request.UnitMeasureId;
            product.ProductionOrigin = request.ProductionOrigin;

            // === PRODUCT TYPES ===
            var defaultCurrency = await context.Currencies
                .FirstOrDefaultAsync(c => c.IsDefault || c.Code == "UZS", ct)
                ?? throw new NotFoundException("Currency", "IsDefault", true);

            var incomingTypeIds = request.ProductTypes
                .Where(t => t.Id > 0)
                .Select(t => t.Id)
                .ToHashSet();

            // O'chirilishi kerak bo'lgan type'lar (request'da yo'q bo'lganlar)
            var typesToRemove = product.ProductTypes
                .Where(t => t.Id > 0 && !incomingTypeIds.Contains(t.Id))
                .ToList();

            foreach (var typeToRemove in typesToRemove)
            {
                await CascadeDeleteProductType(typeToRemove.Id, ct);
            }

            // Mavjud type'larni yangilash va yangilarini qo'shish
            foreach (var typeCmd in request.ProductTypes)
            {
                if (typeCmd.Id > 0)
                {
                    // Mavjud type'ni yangilash
                    var existingType = await context.ProductTypes
                        .FirstOrDefaultAsync(t => t.Id == typeCmd.Id && t.ProductId == product.Id, ct);

                    if (existingType is null) continue;

                    // ✅ VALIDATSIYA: BundleItemCount va UnitPrice tekshirish
                    if (typeCmd.BundleItemCount <= 0)
                        throw new AppException($"'{typeCmd.Type}' turi uchun qopdagi dona soni (BundleItemCount) 0 dan katta bo'lishi shart!");

                    if (typeCmd.UnitPrice < 0)
                        throw new AppException($"'{typeCmd.Type}' turi uchun birlik narxi (UnitPrice) manfiy bo'lishi mumkin emas!");

                    // BundleItemCount o'zgarganmi tekshirish
                    if (existingType.BundleItemCount != typeCmd.BundleItemCount)
                    {
                        await UpdateEntriesForBundleChange(existingType.Id, typeCmd.BundleItemCount, ct);
                    }

                    existingType.Type = typeCmd.Type;
                    existingType.BundleItemCount = typeCmd.BundleItemCount;
                    existingType.PackItemCount = typeCmd.PackItemCount;
                    existingType.UnitPrice = typeCmd.UnitPrice;
                }
                else
                {
                    // Yangi type qo'shish
                    if (string.IsNullOrWhiteSpace(typeCmd.Type)) continue;

                    // ✅ VALIDATSIYA: BundleItemCount va UnitPrice tekshirish
                    if (typeCmd.BundleItemCount <= 0)
                        throw new AppException($"'{typeCmd.Type}' turi uchun qopdagi dona soni (BundleItemCount) 0 dan katta bo'lishi shart!");

                    if (typeCmd.UnitPrice < 0)
                        throw new AppException($"'{typeCmd.Type}' turi uchun birlik narxi (UnitPrice) manfiy bo'lishi mumkin emas!");

                    var newType = new ProductType
                    {
                        Type = typeCmd.Type,
                        BundleItemCount = typeCmd.BundleItemCount,
                        PackItemCount = typeCmd.PackItemCount,
                        UnitPrice = typeCmd.UnitPrice,
                        ProductId = product.Id,
                        Product = product,
                        Currency = defaultCurrency,
                        ProductEntries = []
                    };

                    context.ProductTypes.Add(newType);
                }
            }

            await context.SaveAsync(ct);

            var typesNeedingBarcode = await context.ProductTypes
                .Where(t => t.ProductId == product.Id && (t.QopBarcode == null || t.PackBarcode == null))
                .ToListAsync(ct);

            foreach (var productType in typesNeedingBarcode)
                BarcodeGenerator.EnsureBarcodes(productType);

            await context.CommitTransactionAsync(ct);

            // ✅ TRANSACTION MUVAFFAQIYATLI BO'LGANDAN KEYIN eski rasmni o'chirish
            if (!string.IsNullOrWhiteSpace(imagePathToDelete))
                await TryDeleteFileAsync(imagePathToDelete, ct);

            return true;
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// BundleItemCount o'zgarganda barcha entry'larni qayta hisoblash.
    /// Agar type savdoda qatnashgan bo'lsa — xatolik.
    /// </summary>
    private async Task UpdateEntriesForBundleChange(long productTypeId, int newBundleItemCount, CancellationToken ct)
    {
        var hasSales = await context.SaleItems
            .AnyAsync(si => si.ProductTypeId == productTypeId, ct);

        if (hasSales)
            throw new ForbiddenException(
                "Bu turdagi mahsulot savdoda qatnashgan. BundleItemCount ni o'zgartirib bo'lmaydi!");

        var entries = await context.ProductEntries
            .Where(e => e.ProductTypeId == productTypeId)
            .ToListAsync(ct);

        foreach (var entry in entries)
        {
            // BundleCount (qoplar soni) o'zgarmaydi, faqat BundleItemCount yangilanadi
            var bundleCount = entry.BundleItemCount > 0
                ? entry.Count / entry.BundleItemCount
                : entry.Count;

            entry.BundleItemCount = newBundleItemCount;
            entry.Count = bundleCount * newBundleItemCount;
            entry.TotalAmount = entry.Count * entry.UnitPrice;
        }

        // Residue'ni ham yangilash
        var residues = await context.ProductResidues
            .Where(r => r.ProductTypeId == productTypeId)
            .ToListAsync(ct);

        foreach (var residue in residues)
        {
            var totalEntryCount = entries
                .Where(e => e.ProductResidueId == residue.Id)
                .Sum(e => e.Count);

            var totalSoldCount = await context.SaleItems
                .Where(si => si.ProductTypeId == productTypeId)
                .SumAsync(si => si.TotalCount, ct);

            residue.Count = totalEntryCount - totalSoldCount;
        }
    }

    /// <summary>
    /// ProductType va unga bog'liq barcha ma'lumotlarni cascade o'chirish.
    /// Savdoda qatnashganligini tekshiradi.
    /// </summary>
    private async Task CascadeDeleteProductType(long productTypeId, CancellationToken ct)
    {
        var hasSales = await context.SaleItems
            .AnyAsync(si => si.ProductTypeId == productTypeId, ct);

        if (hasSales)
            throw new ForbiddenException(
                "Bu turdagi mahsulot savdoda qatnashgan. O'chirib bo'lmaydi!");

        // ProductEntry
        var entries = await context.ProductEntries
            .Where(e => e.ProductTypeId == productTypeId)
            .ToListAsync(ct);
        context.ProductEntries.RemoveRange(entries);

        // ProductResidue
        var residues = await context.ProductResidues
            .Where(r => r.ProductTypeId == productTypeId)
            .ToListAsync(ct);
        context.ProductResidues.RemoveRange(residues);

        // ProductType
        var productType = await context.ProductTypes
            .FirstOrDefaultAsync(t => t.Id == productTypeId, ct);

        if (productType is not null)
            context.ProductTypes.Remove(productType);
    }

    private async Task TryDeleteFileAsync(string objectKey, CancellationToken ct)
    {
        try
        {
            await fileStorage.DeleteFileAsync(objectKey, ct);
        }
        catch
        {
            // Image o'chirishda xatolik bo'lsa, asosiy operatsiyani to'xtatmaymiz
        }
    }
}
