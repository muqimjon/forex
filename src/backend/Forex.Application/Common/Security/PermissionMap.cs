namespace Forex.Application.Common.Security;

using Forex.Domain.Enums;

/// <summary>
/// Bo'lim-ruxsatlari matritsasi: MediatR so'rov turi → talab qilingan bo'limlar (OR birlashmasi).
/// Bu yerda YO'Q so'rovlar har qanday autentifikatsiyalangan foydalanuvchi uchun ochiq (lookuplar).
///
/// Prinsip:
///   • Yozish (write) amali  → bo'limning o'zi.
///   • Pul/tarix o'qishlari   → bo'lim YOKI Reports (menejer hisobotlarni ko'ra olishi uchun).
///   • Umumiy lookup o'qishlar (mijoz/valyuta/mahsulot ro'yxati) → ochiq (dropdownlar buzilmasin).
///
/// Diqqat: UpdateUserCommand ATAYIN yo'q — u self-profil tahriri uchun ham ishlatiladi
/// (handler ichida admin/self tekshiruvi bor). Faqat Create/Delete user → Users.
/// </summary>
public static class PermissionMap
{
    private static long M(params AccessPermissions[] sections)
    {
        long mask = 0;
        foreach (var s in sections) mask |= (long)s;
        return mask;
    }

    public static readonly IReadOnlyDictionary<Type, long> Rules = new Dictionary<Type, long>
    {
        // ── Savdo (Sales) ──────────────────────────────────────────────
        [typeof(Features.Sales.Commands.CreateSaleCommand)] = M(AccessPermissions.Sales),
        [typeof(Features.Sales.Commands.UpdateSaleCommand)] = M(AccessPermissions.Sales),
        [typeof(Features.Sales.Commands.DeleteSaleCommand)] = M(AccessPermissions.Sales),
        [typeof(Features.Sales.Queries.SaleFilterQuery)] = M(AccessPermissions.Sales, AccessPermissions.Reports),
        [typeof(Features.Sales.Queries.GetAllSalesQuery)] = M(AccessPermissions.Sales, AccessPermissions.Reports),
        [typeof(Features.Sales.Queries.GetSaleByIdQuery)] = M(AccessPermissions.Sales, AccessPermissions.Reports),
        [typeof(Features.Sales.Queries.GetSaleDocumentSummaryQuery)] = M(AccessPermissions.Sales, AccessPermissions.Reports),

        // ── Qaytarish (Returns) ────────────────────────────────────────
        [typeof(Features.Returns.Commands.CreateReturnCommand)] = M(AccessPermissions.Returns),
        [typeof(Features.Returns.Commands.UpdateReturnCommand)] = M(AccessPermissions.Returns),
        [typeof(Features.Returns.Commands.DeleteReturnCommand)] = M(AccessPermissions.Returns),
        [typeof(Features.Returns.Queries.ReturnFilterQuery)] = M(AccessPermissions.Returns, AccessPermissions.Reports),
        [typeof(Features.Returns.Queries.GetAllReturnsQuery)] = M(AccessPermissions.Returns, AccessPermissions.Reports),
        [typeof(Features.Returns.Queries.GetReturnByIdQuery)] = M(AccessPermissions.Returns, AccessPermissions.Reports),

        // ── To'lov (Payments / Transaction) ────────────────────────────
        [typeof(Features.Transactions.Commands.CreateTransactionCommand)] = M(AccessPermissions.Payments),
        [typeof(Features.Transactions.Commands.UpdateTransactionCommand)] = M(AccessPermissions.Payments),
        [typeof(Features.Transactions.Commands.DeleteTransactionCommand)] = M(AccessPermissions.Payments),
        [typeof(Features.Transactions.Commands.LinkPaymentsToSaleCommand)] = M(AccessPermissions.Payments),
        [typeof(Features.Transactions.Queries.TransactionFilterQuery)] = M(AccessPermissions.Payments, AccessPermissions.Reports),
        [typeof(Features.Transactions.Queries.GetAllTransactionsQuery)] = M(AccessPermissions.Payments, AccessPermissions.Reports),
        [typeof(Features.Transactions.Queries.GetUnlinkedPaymentsQuery)] = M(AccessPermissions.Payments, AccessPermissions.Reports),

        // ── Hisobot (Reports) — aylanma/operatsiyalar ──────────────────
        [typeof(Features.OperationRecords.Queries.OperationRecordFilterQuery)] = M(AccessPermissions.Reports),
        [typeof(Features.OperationRecords.Queries.GetAllOperationRecordsQuery)] = M(AccessPermissions.Reports),
        [typeof(Features.OperationRecords.Queries.GetOperationRecordByIdQuery)] = M(AccessPermissions.Reports),
        [typeof(Features.OperationRecords.Queries.GetOperationRecordByUserIdQuery)] = M(AccessPermissions.Reports),

        // ── Mahsulot (Products) — kirim va mahsulot boshqaruvi ─────────
        [typeof(Features.Products.ProductEntries.Commands.CreateProductEntryCommand)] = M(AccessPermissions.Products),
        [typeof(Features.Products.ProductEntries.Commands.UpdateProductEntryCommand)] = M(AccessPermissions.Products),
        [typeof(Features.Products.ProductEntries.Commands.DeleteProductEntryCommand)] = M(AccessPermissions.Products),
        [typeof(Features.Products.Products.Commands.CreateProductCommand)] = M(AccessPermissions.Products),
        [typeof(Features.Products.Products.Commands.UpdateProductCommand)] = M(AccessPermissions.Products),
        [typeof(Features.Products.Products.Commands.DeleteProductCommand)] = M(AccessPermissions.Products),
        [typeof(Features.Products.ProductTypes.Commands.DeleteProductTypeCommand)] = M(AccessPermissions.Products),

        // ── Barkod (Barcode) ───────────────────────────────────────────
        [typeof(Features.Products.ProductTypes.Commands.GenerateMissingBarcodesCommand)] = M(AccessPermissions.Barcode),

        // ── Ta'minot (Supply) ──────────────────────────────────────────
        [typeof(Features.Supplies.Commands.CreateSupplyCommand)] = M(AccessPermissions.Supply),
        [typeof(Features.Supplies.Commands.UpdateSupplyCommand)] = M(AccessPermissions.Supply),
        [typeof(Features.Supplies.Commands.DeleteSupplyCommand)] = M(AccessPermissions.Supply),
        [typeof(Features.Supplies.Queries.GetAllSuppliesQuery)] = M(AccessPermissions.Supply, AccessPermissions.Reports),

        // ── User boshqaruvi (Users) — Create/Delete (Update self-profil uchun ochiq) ──
        [typeof(Features.Users.Commands.CreateUserCommand)] = M(AccessPermissions.Users),
        [typeof(Features.Users.Commands.DeleteUserCommand)] = M(AccessPermissions.Users),
    };
}
