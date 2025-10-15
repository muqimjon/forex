namespace Forex.Application.Features.Sales.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteSaleCommand(long Id) : IRequest<bool>;

public class DeleteSaleCommandHandler(
    IAppDbContext context) : IRequestHandler<DeleteSaleCommand, bool>
{
    public async Task<bool> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            // === 1️⃣ Sale ni yuklaymiz (itemlari bilan) ===
            var sale = await context.Sales
                .Include(s => s.SaleItems)
                .Include(s => s.User)
                    .ThenInclude(u => u.Accounts)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Sale), nameof(request.Id), request.Id);

            var userAccount = sale.User.Accounts.FirstOrDefault()
                ?? throw new NotFoundException(nameof(UserAccount), nameof(sale.UserId), sale.UserId);

            // === 2️⃣ Foydalanuvchi balansini tiklaymiz ===
            userAccount.Balance += sale.TotalAmount;

            // === 3️⃣ Qoldiqlarni tiklaymiz ===
            var productTypeIds = sale.SaleItems.Select(i => i.ProductTypeId).ToList();

            var productResidues = await context.ProductResidues
                .Where(p => productTypeIds.Contains(p.ProductTypeId))
                .ToListAsync(cancellationToken);

            foreach (var item in sale.SaleItems)
            {
                var residue = productResidues.FirstOrDefault(r => r.ProductTypeId == item.ProductTypeId);
                if (residue is not null)
                {
                    residue.TypeCount += item.TypeCount; // qoldiqni qaytarib qo'yamiz
                }
            }

            // === 4️⃣ SaleItem larni o‘chiramiz ===
            context.SaleItems.RemoveRange(sale.SaleItems);

            // === 5️⃣ Sale ni o‘chiramiz ===
            context.Sales.Remove(sale);

            // === 6️⃣ Tranzaksiyani commit qilamiz ===
            await context.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
