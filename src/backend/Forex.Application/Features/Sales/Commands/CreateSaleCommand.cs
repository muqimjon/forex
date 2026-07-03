namespace Forex.Application.Features.Sales.Commands;

using AutoMapper;
using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Sales.SaleItems.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.Sales;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

public record CreateSaleCommand(
    DateTime Date,
    long CustomerId,
    decimal TotalAmount,
    string? Note,
    List<SaleItemCommand> SaleItems)
    : IRequest<long>;

public class CreateSaleCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<CreateSaleCommand, long>
{
    public async Task<long> Handle(CreateSaleCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            var customer = await context.Users.FirstOrDefaultAsync(u => u.Id == request.CustomerId, ct)
                ?? throw new NotFoundException(nameof(User), nameof(request.CustomerId), request.CustomerId);

            var productResidues = await LoadProductResiduesAsync(request.SaleItems, ct);

            var sale = CreateSale(request);
            sale.CurrencyId = customer.SettlementCurrencyId;

            var saleItems = BuildSaleItems(request.SaleItems, productResidues, sale);

            UpdateProductTypeCounts(request.SaleItems, productResidues);

            CalculateSaleTotals(sale, saleItems);

            sale.SaleItems.Clear();

            context.Sales.Add(sale);
            context.SaleItems.AddRange(saleItems);

            var currency = await context.Currencies
                .FirstOrDefaultAsync(c => c.Id == sale.CurrencyId, ct);
            var currencyCode = currency?.Code ?? string.Empty;
            var baseRate = currency is null || currency.IsDefault || currency.ExchangeRate == 0 ? 1m : currency.ExchangeRate;
            sale.BaseAmount = sale.TotalAmount * baseRate;

            var description = await GenerateDescription(sale, currencyCode);

            var (rate, _) = await context.ApplyToSettlementAsync(
                sale.CustomerId, sale.CurrencyId, 1m, -sale.TotalAmount, ct);

            sale.OperationRecord = new()
            {
                Amount = -sale.TotalAmount,
                Rate = rate,
                CurrencyId = sale.CurrencyId,
                Date = sale.Date,
                Description = description,
                Type = OperationType.Sale,
                UserId = sale.CustomerId
            };

            await context.CommitTransactionAsync(ct);
            return sale.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<string> GenerateDescription(Sale sale, string currencyCode)
    {
        StringBuilder text = new();

        // Birinchi qator — operatsiya savdo ekanini va izohni bildiradi; tagida mahsulotlar keladi.
        text.AppendLine(string.IsNullOrWhiteSpace(sale.Note) ? "Savdo:" : $"Savdo: {sale.Note}");

        foreach (var item in sale.SaleItems)
        {
            var productType = await context.ProductTypes
                      .Include(pt => pt.Product)
                      .FirstOrDefaultAsync(pt => pt.Id == item.ProductTypeId)
                      ?? throw new NotFoundException(nameof(ProductType), nameof(item.ProductTypeId), item.ProductTypeId);

            text.AppendLine($"Kodi: {productType.Product.Code} ({productType.Type}), Soni: {item.TotalCount}, Narxi: {item.UnitPrice}, Jami: {item.Amount} {currencyCode}");
        }

        return text.ToString().TrimEnd();
    }

    private async Task<List<ProductResidue>> LoadProductResiduesAsync(List<SaleItemCommand> saleItems, CancellationToken ct)
    {
        var productTypeIds = saleItems.Select(i => i.ProductTypeId).ToList();

        return await context.ProductResidues
            .Include(p => p.ProductEntries)
            .Include(p => p.ProductType)
            .Where(p => productTypeIds.Contains(p.ProductTypeId))
            .ToListAsync(ct);
    }

    private Sale CreateSale(CreateSaleCommand request)
    {
        return mapper.Map<Sale>(request);
    }

    private List<SaleItem> BuildSaleItems(List<SaleItemCommand> commands, List<ProductResidue> residues, Sale sale)
    {
        var items = new List<SaleItem>();

        foreach (var cmd in commands)
        {
            var residue = residues.FirstOrDefault(r => r.ProductTypeId == cmd.ProductTypeId)
                ?? throw new NotFoundException(nameof(ProductResidue), nameof(cmd.ProductTypeId), cmd.ProductTypeId);

            // Bundle size comes from the ProductType definition, not the last stock intake:
            // a sale must not depend on any ProductEntry existing (orphan/zero-stock residues break otherwise).
            var bundleItemCount = residue.ProductType.BundleItemCount;
            var totalCount = cmd.BundleCount * bundleItemCount;

            items.Add(new SaleItem
            {
                BundleCount = cmd.BundleCount,
                BundleItemCount = bundleItemCount,
                TotalCount = totalCount,
                UnitPrice = cmd.UnitPrice,
                Amount = cmd.Amount,
                ProductTypeId = cmd.ProductTypeId,
                Sale = sale
            });
        }

        return items;
    }

    private static void UpdateProductTypeCounts(List<SaleItemCommand> commands, List<ProductResidue> residues)
    {
        foreach (var cmd in commands)
        {
            var residue = residues.First(r => r.ProductTypeId == cmd.ProductTypeId);
            var totalCount = cmd.BundleCount * residue.ProductType.BundleItemCount;
            if (residue.Count < totalCount)
                throw new ForbiddenException($"Do'konda yetarli mahsulot mavjud emas, jami mahsulot soni {residue.Count}");
            residue.Count -= cmd.BundleCount * residue.ProductType.BundleItemCount;
        }
    }

    private static void CalculateSaleTotals(Sale sale, List<SaleItem> items)
    {
        sale.TotalCount = items.Sum(s => s.TotalCount);
    }
}
