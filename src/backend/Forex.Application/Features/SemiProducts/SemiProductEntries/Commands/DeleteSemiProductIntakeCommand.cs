namespace Forex.Application.Features.SemiProducts.SemiProductEntries.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.SemiProducts;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteSemiProductIntakeCommand(long InvoiceId) : IRequest<bool>;

public class DeleteSemiProductIntakeCommandHandler(
    IAppDbContext context
) : IRequestHandler<DeleteSemiProductIntakeCommand, bool>
{
    public async Task<bool> Handle(DeleteSemiProductIntakeCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            var invoice = await GetInvoiceWithRelatedDataAsync(request.InvoiceId, ct);
            var manufactory = GetManufactory(invoice);
            var semiProductEntries = GetSemiProductEntriesByInvoice(manufactory, invoice.Id);

            await ValidateSemiProductResiduesAsync(semiProductEntries, ct);

            var semiProductIds = semiProductEntries.Select(e => e.SemiProductId).Distinct().ToList();

            await DeleteProductTypeItemsAsync(semiProductIds, ct);
            await DeleteProductTypesAsync(semiProductIds, ct);
            await DeleteProductsAsync(semiProductIds, ct);
            DeleteSemiProductResidues(manufactory, semiProductIds);
            DeleteSemiProductEntries(semiProductEntries);
            await DeleteSemiProductsAsync(semiProductIds, ct);
            await RevertPaymentsAsync(invoice.Payments, ct);
            DeletePayments(invoice.Payments);
            DeleteInvoice(invoice);

            return await context.CommitTransactionAsync(ct);
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<Invoice> GetInvoiceWithRelatedDataAsync(long invoiceId, CancellationToken ct)
    {
        return await context.Invoices
            .Include(i => i.Payments)
            .Include(i => i.Manufactory)
                .ThenInclude(m => m!.SemiProductEntries)
            .Include(i => i.Manufactory)
                .ThenInclude(m => m!.SemiProductResidues)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct)
            ?? throw new NotFoundException(nameof(Invoice), nameof(invoiceId), invoiceId);
    }

    private static Manufactory GetManufactory(Invoice invoice)
    {
        return invoice.Manufactory
            ?? throw new NotFoundException("Manufactory topilmadi");
    }

    private static List<SemiProductEntry> GetSemiProductEntriesByInvoice(Manufactory manufactory, long invoiceId)
    {
        return manufactory.SemiProductEntries
            .Where(e => e.InvoiceId == invoiceId)
            .ToList();
    }

    private async Task ValidateSemiProductResiduesAsync(List<SemiProductEntry> entries, CancellationToken ct)
    {
        foreach (var entry in entries)
        {
            var residue = await context.SemiProductResidues
                .FirstOrDefaultAsync(r => r.SemiProductId == entry.SemiProductId, ct);

            if (residue is null || residue.Quantity < entry.Quantity)
            {
                var semiProduct = await context.SemiProducts
                    .FirstOrDefaultAsync(s => s.Id == entry.SemiProductId, ct);

                throw new ForbiddenException(
                    $"Yarim tayyor mahsulot '{semiProduct?.Name}' qoldig'i yetarli emas. " +
                    $"Kerak: {entry.Quantity}, Mavjud: {residue?.Quantity ?? 0}");
            }
        }
    }

    private async Task DeleteProductTypeItemsAsync(List<long> semiProductIds, CancellationToken ct)
    {
        var productTypeItems = await context.ProductTypeItems
            .Where(pti => semiProductIds.Contains(pti.SemiProductId))
            .ToListAsync(ct);

        context.ProductTypeItems.RemoveRange(productTypeItems);
    }

    private async Task DeleteProductTypesAsync(List<long> semiProductIds, CancellationToken ct)
    {
        var productTypeItemIds = await context.ProductTypeItems
            .Where(pti => semiProductIds.Contains(pti.SemiProductId))
            .Select(pti => pti.ProductTypeId)
            .Distinct()
            .ToListAsync(ct);

        var productTypes = await context.ProductTypes
            .Where(pt => productTypeItemIds.Contains(pt.Id))
            .ToListAsync(ct);

        context.ProductTypes.RemoveRange(productTypes);
    }

    private async Task DeleteProductsAsync(List<long> semiProductIds, CancellationToken ct)
    {
        var productIds = await context.ProductTypeItems
            .Where(pti => semiProductIds.Contains(pti.SemiProductId))
            .Join(context.ProductTypes,
                pti => pti.ProductTypeId,
                pt => pt.Id,
                (pti, pt) => pt.ProductId)
            .Distinct()
            .ToListAsync(ct);

        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        context.Products.RemoveRange(products);
    }

    private void DeleteSemiProductResidues(Manufactory manufactory, List<long> semiProductIds)
    {
        var residues = manufactory.SemiProductResidues
            .Where(r => semiProductIds.Contains(r.SemiProductId))
            .ToList();

        context.SemiProductResidues.RemoveRange(residues);
    }

    private void DeleteSemiProductEntries(List<SemiProductEntry> entries)
    {
        context.SemiProductEntries.RemoveRange(entries);
    }

    private async Task DeleteSemiProductsAsync(List<long> semiProductIds, CancellationToken ct)
    {
        var semiProducts = await context.SemiProducts
            .Where(s => semiProductIds.Contains(s.Id))
            .ToListAsync(ct);

        context.SemiProducts.RemoveRange(semiProducts);
    }

    private async Task RevertPaymentsAsync(ICollection<InvoicePayment> payments, CancellationToken ct)
    {
        foreach (var payment in payments)
        {
            var userAccount = await context.UserAccounts
                .FirstOrDefaultAsync(ua => ua.UserId == payment.UserId && ua.CurrencyId == payment.CurrencyId, ct);

            if (userAccount is not null)
            {
                userAccount.Balance -= payment.Amount;

                if (userAccount.Balance == 0 && userAccount.OpeningBalance == payment.Amount)
                {
                    context.UserAccounts.Remove(userAccount);
                }
            }
        }
    }

    private void DeletePayments(ICollection<InvoicePayment> payments)
    {
        context.InvoicePayments.RemoveRange(payments);
    }

    private void DeleteInvoice(Invoice invoice)
    {
        context.Invoices.Remove(invoice);
    }
}