namespace Forex.Application.Features.Returns.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Returns.ReturnItems.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.Sales;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

public record UpdateReturnCommand(
    long Id,
    DateTime Date,
    long CustomerId,
    decimal TotalAmount,
    string? Note,
    List<ReturnItemCommand> ReturnItems)
    : IRequest<bool>;

public class UpdateReturnCommandHandler(
    IAppDbContext context)
    : IRequestHandler<UpdateReturnCommand, bool>
{
    public async Task<bool> Handle(UpdateReturnCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            var @return = await LoadReturnWithRelationsAsync(request.Id, ct);

            await RevertReturnEffectsAsync(@return, ct);

            await ApplyNewReturnDataAsync(@return, request, ct);

            return await context.CommitTransactionAsync(ct);
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<Return> LoadReturnWithRelationsAsync(long id, CancellationToken ct)
    {
        return await context.Returns
            .Include(r => r.ReturnItems)
            .Include(r => r.Customer)
                .ThenInclude(u => u.Accounts)
            .Include(r => r.OperationRecord)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException(nameof(Return), nameof(id), id);
    }

    private async Task RevertReturnEffectsAsync(Return @return, CancellationToken ct)
    {
        if (@return.OperationRecord is not null)
        {
            var account = await context.GetSettlementAccountAsync(@return.CustomerId, ct);
            account.Balance -= @return.OperationRecord.Amount * @return.OperationRecord.Rate;
        }

        var productTypeIds = @return.ReturnItems.Select(ri => ri.ProductTypeId).Distinct().ToList();
        var productResidues = await LoadProductResiduesAsync(productTypeIds, ct);
        DeductProductResidues(@return.ReturnItems, productResidues);

        context.ReturnItems.RemoveRange(@return.ReturnItems);
        @return.ReturnItems.Clear();
    }

    private async Task ApplyNewReturnDataAsync(Return @return, UpdateReturnCommand request, CancellationToken ct)
    {
        var customer = await context.Users.FirstOrDefaultAsync(u => u.Id == request.CustomerId, ct)
            ?? throw new NotFoundException(nameof(User), nameof(request.CustomerId), request.CustomerId);

        var productTypeIds = request.ReturnItems.Select(i => i.ProductTypeId).Distinct().ToList();
        var productResidues = await LoadProductResiduesAsync(productTypeIds, ct);

        @return.Date = request.Date.ToUtcSafe();
        @return.CustomerId = request.CustomerId;
        @return.CurrencyId = customer.SettlementCurrencyId;
        @return.TotalAmount = request.TotalAmount;
        @return.Note = request.Note;

        var returnItems = BuildReturnItems(request.ReturnItems, productResidues, @return);

        RestoreProductResidues(returnItems, productResidues);

        @return.TotalCount = returnItems.Sum(i => i.TotalCount);

        @return.ReturnItems = returnItems;

        var currency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Id == @return.CurrencyId, ct);
        var currencyCode = currency?.Code ?? string.Empty;
        var baseRate = currency is null || currency.IsDefault || currency.ExchangeRate == 0 ? 1m : currency.ExchangeRate;
        @return.BaseAmount = @return.TotalAmount * baseRate;

        var description = await GenerateDescriptionAsync(returnItems, currencyCode, @return.Note, ct);

        var (rate, _) = await context.ApplyToSettlementAsync(
            @return.CustomerId, @return.CurrencyId, 1m, @return.TotalAmount, ct);

        var record = @return.OperationRecord ?? new OperationRecord();
        record.Amount = @return.TotalAmount;
        record.Rate = rate;
        record.CurrencyId = @return.CurrencyId;
        record.Date = @return.Date;
        record.Description = description;
        record.Type = OperationType.Return;
        record.UserId = @return.CustomerId;
        @return.OperationRecord = record;

        context.Returns.Update(@return);
    }

    private async Task<string> GenerateDescriptionAsync(List<ReturnItem> returnItems, string currencyCode, string? note, CancellationToken ct)
    {
        var text = new StringBuilder();

        // Birinchi qator — operatsiya qaytarish ekanini va izohni bildiradi; tagida mahsulotlar keladi.
        text.AppendLine(string.IsNullOrWhiteSpace(note) ? "Qaytarildi:" : $"Qaytarildi: {note}");

        var productTypeIds = returnItems.Select(i => i.ProductTypeId).ToList();

        var productTypes = await context.ProductTypes
            .Include(pt => pt.Product)
            .Where(pt => productTypeIds.Contains(pt.Id))
            .ToListAsync(ct);

        foreach (var item in returnItems)
        {
            var productType = productTypes.FirstOrDefault(pt => pt.Id == item.ProductTypeId)
                ?? throw new NotFoundException(nameof(ProductType), nameof(item.ProductTypeId), item.ProductTypeId);

            text.AppendLine($"Kodi: {productType.Product.Code} ({productType.Type}), Soni: {item.TotalCount}, Narxi: {item.UnitPrice}, Jami: {item.Amount} {currencyCode}");
        }

        return text.ToString().TrimEnd();
    }

    private async Task<List<ProductResidue>> LoadProductResiduesAsync(List<long> productTypeIds, CancellationToken ct)
    {
        return await context.ProductResidues
            .Include(p => p.ProductType)
            .Where(p => productTypeIds.Contains(p.ProductTypeId))
            .ToListAsync(ct);
    }

    private List<ReturnItem> BuildReturnItems(List<ReturnItemCommand> commands, List<ProductResidue> residues, Return @return)
    {
        var items = new List<ReturnItem>();

        foreach (var cmd in commands.Where(c => c.TotalCount > 0 || c.BundleCount > 0))
        {
            var residue = residues.FirstOrDefault(r => r.ProductTypeId == cmd.ProductTypeId);
            var bundleItemCount = residue?.ProductType.BundleItemCount ?? 0;
            var totalCount = cmd.TotalCount > 0 ? cmd.TotalCount : cmd.BundleCount * bundleItemCount;

            items.Add(new ReturnItem
            {
                BundleCount = cmd.BundleCount,
                BundleItemCount = bundleItemCount,
                TotalCount = totalCount,
                RestockCount = Math.Clamp(cmd.RestockCount, 0, totalCount),
                UnitPrice = cmd.UnitPrice,
                Amount = cmd.Amount,
                ProductTypeId = cmd.ProductTypeId,
                Return = @return
            });
        }

        return items;
    }

    private void RestoreProductResidues(IEnumerable<ReturnItem> returnItems, List<ProductResidue> residues)
    {
        foreach (var item in returnItems)
        {
            if (item.RestockCount <= 0) continue;
            var residue = residues.FirstOrDefault(r => r.ProductTypeId == item.ProductTypeId);
            if (residue is null) continue;
            residue.Count += item.RestockCount;
        }
    }

    private void DeductProductResidues(IEnumerable<ReturnItem> returnItems, List<ProductResidue> residues)
    {
        foreach (var item in returnItems)
        {
            if (item.RestockCount <= 0) continue;
            var residue = residues.FirstOrDefault(r => r.ProductTypeId == item.ProductTypeId);
            if (residue is null) continue;
            residue.Count -= item.RestockCount;
        }
    }
}
