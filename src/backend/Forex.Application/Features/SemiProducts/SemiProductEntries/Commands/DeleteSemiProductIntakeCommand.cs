namespace Forex.Application.Features.SemiProducts.SemiProductEntries.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
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
            var invoice = await LoadInvoiceAsync(request.InvoiceId, ct);
            var manufactory = await LoadManufactoryAsync(invoice.ManufactoryId, ct);

            await ValidateResiduesAsync(invoice, manufactory.Id, ct);
            await ValidateBalancesAsync(invoice, ct);

            await RevertResiduesAsync(invoice, manufactory.Id, ct);
            await RevertBalancesAsync(invoice, ct);
            await RemoveSemiProductsAsync(invoice, ct);
            await RemoveInvoiceAsync(invoice, ct);

            return await context.CommitTransactionAsync(ct);
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    // 🔹 1. Invoice va Entry’larni yuklash
    private async Task<Invoice> LoadInvoiceAsync(long invoiceId, CancellationToken ct)
    {
        return await context.Invoices
            .Include(i => i.SemiProductEntries)
                .ThenInclude(e => e.SemiProduct)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct)
            ?? throw new NotFoundException(nameof(Invoice), nameof(invoiceId), invoiceId);
    }

    // 🔹 2. Manufaktura yuklash
    private async Task<Manufactory> LoadManufactoryAsync(long manufactoryId, CancellationToken ct)
    {
        return await context.Manufactories
            .FirstOrDefaultAsync(m => m.Id == manufactoryId, ct)
            ?? throw new ForbiddenException("Manufaktura topilmadi.");
    }

    // 🔹 3. Qoldiq yetarliligini tekshirish
    private async Task ValidateResiduesAsync(Invoice invoice, long manufactoryId, CancellationToken ct)
    {
        foreach (var entry in invoice.SemiProductEntries)
        {
            var residue = await context.SemiProductResidues
                .FirstOrDefaultAsync(r => r.SemiProductId == entry.SemiProductId && r.ManufactoryId == manufactoryId, ct)
                ?? throw new ForbiddenException($"Qoldiq topilmadi: {entry.SemiProduct.Name}");

            if (residue.Quantity < entry.Quantity)
                throw new ForbiddenException($"Qoldiq yetarli emas: {entry.SemiProduct.Name}");
        }
    }

    // 🔹 4. Balans yetarliligini tekshirish
    private async Task ValidateBalancesAsync(Invoice invoice, CancellationToken ct)
    {
        if (invoice.ViaMiddleman && invoice.TransferFee is > 0)
        {
            var sender = await context.Users
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.Id == invoice.SenderId, ct)
                ?? throw new NotFoundException(nameof(User), nameof(invoice.SenderId), invoice.SenderId!);

            var senderAccount = sender.Accounts.FirstOrDefault(a => a.CurrencyId == invoice.CurrencyId)
                ?? throw new ForbiddenException("Vositachi hisob topilmadi.");

            if (senderAccount.Balance < invoice.TransferFee)
                throw new ForbiddenException("Vositachi balansida yetarli mablag‘ yo‘q.");
        }

        var supplier = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == invoice.SupplierId, ct)
            ?? throw new NotFoundException(nameof(User), nameof(invoice.SupplierId), invoice.SupplierId);

        var account = supplier.Accounts.FirstOrDefault(a => a.CurrencyId == invoice.CurrencyId)
            ?? throw new ForbiddenException("Ta’minotchi hisob topilmadi.");

        if (account.Balance < invoice.CostPrice)
            throw new ForbiddenException("Ta’minotchi balansida yetarli mablag‘ yo‘q.");
    }

    // 🔹 5. Qoldiqlarni kamaytirish
    private async Task RevertResiduesAsync(Invoice invoice, long manufactoryId, CancellationToken ct)
    {
        foreach (var entry in invoice.SemiProductEntries)
        {
            var residue = await context.SemiProductResidues
                .FirstOrDefaultAsync(r => r.SemiProductId == entry.SemiProductId && r.ManufactoryId == manufactoryId, ct);

            residue!.Quantity -= entry.Quantity;
        }
    }

    // 🔹 6. Balanslarni teskari hisoblash
    private async Task RevertBalancesAsync(Invoice invoice, CancellationToken ct)
    {
        if (invoice.ViaMiddleman && invoice.TransferFee is > 0)
        {
            var sender = await context.Users
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.Id == invoice.SenderId, ct);

            var senderAccount = sender!.Accounts.First(a => a.CurrencyId == invoice.CurrencyId);
            senderAccount.Balance -= (decimal)invoice.TransferFee!;
        }

        var supplier = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == invoice.SupplierId, ct);

        var account = supplier!.Accounts.First(a => a.CurrencyId == invoice.CurrencyId);
        account.Balance -= invoice.CostPrice;
    }

    // 🔹 7. ProductTypeId va Entry’larni o‘chirish
    private Task RemoveSemiProductsAsync(Invoice invoice, CancellationToken ct)
    {
        var semiProducts = invoice.SemiProductEntries
            .Select(e => e.SemiProduct)
            .Distinct()
            .ToList();

        context.SemiProductEntries.RemoveRange(invoice.SemiProductEntries);
        context.SemiProducts.RemoveRange(semiProducts);

        return Task.CompletedTask;
    }

    private Task RemoveInvoiceAsync(Invoice invoice, CancellationToken ct)
    {
        context.Invoices.Remove(invoice);
        return Task.CompletedTask;
    }
}
