namespace Forex.Application.Features.Sales.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Sales.SaleItems.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateSaleCommand(
    DateTime Date,
    long CustomerId,
    decimal TotalAmount,
    string? Note,
    List<SaleItemCommand> SaleItems)
    : IRequest<long>;

public class CreateSaleCommandHandler(IAppDbContext context, IMapper mapper)
    : IRequestHandler<CreateSaleCommand, long>
{
    public async Task<long> Handle(CreateSaleCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            var userAccount = await GetOrCreateUserAccountAsync(request.CustomerId, request.TotalAmount, ct);
            userAccount.Balance -= request.TotalAmount;

            var productResidues = await LoadProductResiduesAsync(request.SaleItems, ct);

            var sale = CreateSale(request);

            var saleItems = BuildSaleItems(request.SaleItems, productResidues, sale);

            UpdateProductTypeCounts(request.SaleItems, productResidues);

            CalculateSaleTotals(sale, saleItems);

            sale.SaleItems.Clear();

            context.Sales.Add(sale);
            context.SaleItems.AddRange(saleItems);

            await context.CommitTransactionAsync(ct);
            return sale.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
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
        await context.SaveAsync(ct);
        return newAccount;
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

            var entry = residue.ProductEntries.LastOrDefault()
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

    private static void UpdateProductTypeCounts(List<SaleItemCommand> commands, List<ProductResidue> residues)
    {
        foreach (var cmd in commands)
        {
            var residue = residues.First(r => r.ProductTypeId == cmd.ProductTypeId);
            residue.Count -= cmd.BundleCount;
        }
    }

    private static void CalculateSaleTotals(Sale sale, List<SaleItem> items)
    {
        sale.CostPrice = items.Sum(s => s.CostPrice);
        sale.BenifitPrice = items.Sum(s => s.Benifit);
        sale.TotalCount = items.Sum(s => s.TotalCount);
    }
}
