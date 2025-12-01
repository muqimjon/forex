namespace Forex.Application.Features.Sales.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Sales.SaleItems.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.Sales;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

public record UpdateSaleCommand(
    long Id,
    DateTime Date,
    long CustomerId,
    decimal TotalAmount,
    string? Note,
    List<SaleItemCommand> SaleItems)
    : IRequest<bool>;

public class UpdateSaleCommandHandler(
    IAppDbContext context)
    : IRequestHandler<UpdateSaleCommand, bool>
{
    public async Task<bool> Handle(UpdateSaleCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            var sale = await LoadSaleWithRelationsAsync(request.Id, ct);

            // 1) Revert previous effects (balance, stock, items)
            await RevertSaleEffectsAsync(sale, ct);

            // 2) Apply new data deterministically
            await ApplyNewSaleDataAsync(sale, request, ct);

            // 3) Commit
            return await context.CommitTransactionAsync(ct);
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    // --- Load ---

    private async Task<Sale> LoadSaleWithRelationsAsync(long id, CancellationToken ct)
    {
        return await context.Sales
            .Include(s => s.SaleItems)
            .Include(s => s.Customer)
                .ThenInclude(u => u.Accounts)
            .Include(s => s.OperationRecord)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(nameof(Sale), nameof(id), id);
    }

    private async Task RevertSaleEffectsAsync(Sale sale, CancellationToken ct)
    {
        // 1) Return account balance
        var userAccount = sale.Customer.Accounts.FirstOrDefault()
            ?? throw new NotFoundException(nameof(UserAccount), nameof(sale.CustomerId), sale.CustomerId);
        userAccount.Balance += sale.TotalAmount;

        // 2) Return product residues
        var productTypeIds = sale.SaleItems.Select(si => si.ProductTypeId).Distinct().ToList();
        var productResidues = await LoadProductResiduesAsync(productTypeIds, ct);
        RestoreProductResidues(sale.SaleItems, productResidues);

        // 3) Remove old items (navigation and DbSet)
        context.SaleItems.RemoveRange(sale.SaleItems);
        sale.SaleItems.Clear();

        // Note: keep existing OperationRecord to update; do not remove it here.
    }

    // --- Apply ---

    private async Task ApplyNewSaleDataAsync(Sale sale, UpdateSaleCommand request, CancellationToken ct)
    {
        // 1) Account balance apply
        var userAccount = await GetOrCreateUserAccountAsync(request.CustomerId, request.TotalAmount, ct);
        userAccount.Balance -= request.TotalAmount;

        // 2) Load residues for new items
        var productTypeIds = request.SaleItems.Select(i => i.ProductTypeId).Distinct().ToList();
        var productResidues = await LoadProductResiduesAsync(productTypeIds, ct);

        // 3) Update scalar fields only
        sale.Date = request.Date.ToUtcSafe();
        sale.CustomerId = request.CustomerId;
        sale.TotalAmount = request.TotalAmount;
        sale.Note = request.Note;

        // 4) Build new items (BundleCount > 0)
        var saleItems = BuildSaleItems(request.SaleItems, productResidues, sale);

        // 5) Deduct residues using consistent bundle sizing (latest entry)
        DeductProductResidues(request.SaleItems, productResidues);

        // 6) Recalculate totals
        CalculateSaleTotals(sale, saleItems);

        // 7) Attach items via navigation only
        sale.SaleItems = saleItems;

        // 8) Update or create OperationRecord (no manual Id assignment)
        var description = await GenerateDescriptionAsync(saleItems, ct);

        if (sale.OperationRecord is not null)
        {
            sale.OperationRecord.Amount = -sale.TotalAmount;
            sale.OperationRecord.Date = sale.Date;
            sale.OperationRecord.Description = description;
            sale.OperationRecord.Type = OperationType.Sale;
        }
        else
        {
            sale.OperationRecord = new OperationRecord
            {
                Amount = -sale.TotalAmount,
                Date = sale.Date,
                Description = description,
                Type = OperationType.Sale,
                // If you have a FK like SaleId in OperationRecord, set it here:
                // SaleId = sale.Id
            };
        }

        context.Sales.Update(sale);
    }

    // --- Helpers ---

    private async Task<string> GenerateDescriptionAsync(List<SaleItem> saleItems, CancellationToken ct)
    {
        var text = new StringBuilder();
        var productTypeIds = saleItems.Select(i => i.ProductTypeId).ToList();

        var productTypes = await context.ProductTypes
            .Include(pt => pt.Product)
            .Where(pt => productTypeIds.Contains(pt.Id))
            .ToListAsync(ct);

        foreach (var item in saleItems)
        {
            var productType = productTypes.FirstOrDefault(pt => pt.Id == item.ProductTypeId)
                ?? throw new NotFoundException(nameof(ProductType), nameof(item.ProductTypeId), item.ProductTypeId);

            text.AppendLine($"Kodi: {productType.Product.Code} ({productType.Type}), Soni: {item.TotalCount}, Narxi: {item.UnitPrice}, Jami: {item.Amount} UZS");
        }

        return text.ToString();
    }

    private async Task<UserAccount> GetOrCreateUserAccountAsync(long customerId, decimal initialBalance, CancellationToken ct)
    {
        var user = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == customerId, ct)
            ?? throw new NotFoundException(nameof(User), nameof(customerId), customerId);

        var account = user.Accounts.FirstOrDefault();
        if (account is not null) return account;

        var newAccount = new UserAccount
        {
            UserId = user.Id,
            Balance = initialBalance
        };

        context.UserAccounts.Add(newAccount);
        return newAccount;
    }

    private async Task<List<ProductResidue>> LoadProductResiduesAsync(List<long> productTypeIds, CancellationToken ct)
    {
        return await context.ProductResidues
            .Include(p => p.ProductEntries)
            .Include(p => p.ProductType)
            .Where(p => productTypeIds.Contains(p.ProductTypeId))
            .ToListAsync(ct);
    }

    private List<SaleItem> BuildSaleItems(List<SaleItemCommand> commands, List<ProductResidue> residues, Sale sale)
    {
        var items = new List<SaleItem>();

        foreach (var cmd in commands.Where(c => c.BundleCount > 0))
        {
            var residue = residues.FirstOrDefault(r => r.ProductTypeId == cmd.ProductTypeId)
                ?? throw new NotFoundException(nameof(ProductResidue), nameof(cmd.ProductTypeId), cmd.ProductTypeId);

            var entry = residue.ProductEntries.OrderByDescending(e => e.Date).FirstOrDefault()
                ?? throw new NotFoundException(nameof(ProductEntry), nameof(residue.ProductTypeId), residue.ProductTypeId);

            var totalCount = cmd.BundleCount * entry.BundleItemCount;
            var benifit = (cmd.UnitPrice - entry.UnitPrice) * totalCount;

            items.Add(new SaleItem
            {
                BundleCount = cmd.BundleCount,
                BundleItemCount = entry.BundleItemCount,
                TotalCount = totalCount,
                UnitPrice = cmd.UnitPrice,
                Amount = cmd.Amount,
                CostPrice = entry.UnitPrice * totalCount,
                Benifit = benifit,
                ProductTypeId = cmd.ProductTypeId,
                Sale = sale
            });
        }

        return items;
    }

    private void RestoreProductResidues(IEnumerable<SaleItem> saleItems, List<ProductResidue> residues)
    {
        foreach (var item in saleItems)
        {
            var residue = residues.FirstOrDefault(r => r.ProductTypeId == item.ProductTypeId)
                ?? throw new NotFoundException(nameof(ProductResidue), nameof(item.ProductTypeId), item.ProductTypeId);

            residue.Count += item.TotalCount;
        }
    }

    private void DeductProductResidues(List<SaleItemCommand> commands, List<ProductResidue> residues)
    {
        foreach (var cmd in commands.Where(c => c.BundleCount > 0))
        {
            var residue = residues.First(r => r.ProductTypeId == cmd.ProductTypeId);

            var entry = residue.ProductEntries.OrderByDescending(e => e.Date).FirstOrDefault()
                ?? throw new NotFoundException(nameof(ProductEntry), nameof(residue.ProductTypeId), residue.ProductTypeId);

            var totalCount = cmd.BundleCount * entry.BundleItemCount;

            if (residue.Count < totalCount)
                throw new ForbiddenException($"Do'konda yetarli mahsulot mavjud emas, jami mahsulot soni {residue.Count}");

            residue.Count -= totalCount;
        }
    }

    private static void CalculateSaleTotals(Sale sale, List<SaleItem> items)
    {
        sale.CostPrice = items.Sum(s => s.CostPrice);
        sale.BenifitPrice = items.Sum(s => s.Benifit);
        sale.TotalCount = items.Sum(s => s.TotalCount);
    }
}
