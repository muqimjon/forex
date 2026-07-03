namespace Forex.Application.Features.Returns.Commands;

using AutoMapper;
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

public record CreateReturnCommand(
    DateTime Date,
    long CustomerId,
    decimal TotalAmount,
    string? Note,
    List<ReturnItemCommand> ReturnItems)
    : IRequest<long>;

public class CreateReturnCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<CreateReturnCommand, long>
{
    public async Task<long> Handle(CreateReturnCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            var customer = await context.Users.FirstOrDefaultAsync(u => u.Id == request.CustomerId, ct)
                ?? throw new NotFoundException(nameof(User), nameof(request.CustomerId), request.CustomerId);

            var productResidues = await LoadProductResiduesAsync(request.ReturnItems, ct);

            var @return = mapper.Map<Return>(request);
            @return.CurrencyId = customer.SettlementCurrencyId;

            var returnItems = BuildReturnItems(request.ReturnItems, productResidues, @return);

            RestoreProductTypeCounts(returnItems, productResidues);

            @return.TotalCount = returnItems.Sum(i => i.TotalCount);

            @return.ReturnItems.Clear();

            context.Returns.Add(@return);
            context.ReturnItems.AddRange(returnItems);

            var currency = await context.Currencies
                .FirstOrDefaultAsync(c => c.Id == @return.CurrencyId, ct);
            var currencyCode = currency?.Code ?? string.Empty;
            var baseRate = currency is null || currency.IsDefault || currency.ExchangeRate == 0 ? 1m : currency.ExchangeRate;
            @return.BaseAmount = @return.TotalAmount * baseRate;

            var description = await GenerateDescription(returnItems, currencyCode, @return.Note, ct);

            var (rate, _) = await context.ApplyToSettlementAsync(
                @return.CustomerId, @return.CurrencyId, 1m, @return.TotalAmount, ct);

            @return.OperationRecord = new()
            {
                Amount = @return.TotalAmount,
                Rate = rate,
                CurrencyId = @return.CurrencyId,
                Date = @return.Date,
                Description = description,
                Type = OperationType.Return,
                UserId = @return.CustomerId
            };

            await context.CommitTransactionAsync(ct);
            return @return.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<string> GenerateDescription(List<ReturnItem> returnItems, string currencyCode, string? note, CancellationToken ct)
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

    private async Task<List<ProductResidue>> LoadProductResiduesAsync(List<ReturnItemCommand> returnItems, CancellationToken ct)
    {
        var productTypeIds = returnItems.Select(i => i.ProductTypeId).ToList();

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

    private static void RestoreProductTypeCounts(List<ReturnItem> items, List<ProductResidue> residues)
    {
        foreach (var item in items)
        {
            if (item.RestockCount <= 0) continue;
            var residue = residues.FirstOrDefault(r => r.ProductTypeId == item.ProductTypeId);
            if (residue is null) continue;
            residue.Count += item.RestockCount;
        }
    }
}
