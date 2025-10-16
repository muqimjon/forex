namespace Forex.Application.Features.Sales.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Sales.SaleItems.DTOs;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateSaleCommand(
    long Id,
    DateTime Date,
    long UserId,
    int TotalCount,
    decimal TotalAmount,
    string? Note,
    List<SaleItemCreateDto> SaleItems
) : IRequest<bool>;

public class UpdateSaleCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<UpdateSaleCommand, bool>
{
    public async Task<bool> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            // === 1️⃣ Eski sale ni topamiz ===
            var sale = await context.Sales
                .Include(s => s.SaleItems)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Sale), nameof(request.Id), request.Id);

            // === 2️⃣ Foydalanuvchini topamiz ===
            var user = await context.Users
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), nameof(request.UserId), request.UserId);

            var userAccount = user.Accounts.FirstOrDefault()
                ?? throw new NotFoundException(nameof(UserAccount), nameof(user.Id), user.Id);

            // === 3️⃣ Balansni qayta hisoblash ===
            // Avval eski summani qaytarib qo'yamiz
            userAccount.Balance += sale.TotalAmount;
            // Endi yangi summani ayiramiz
            userAccount.Balance -= request.TotalAmount;

            // === 4️⃣ ProductResidues ni tayyorlaymiz ===
            var productTypeIds = request.SaleItems.Select(i => i.ProductTypeId).ToList();

            var productResidues = await context.ProductResidues
                .Include(p => p.ProductEntries)
                .Where(p => productTypeIds.Contains(p.ProductType.Id))
                .ToListAsync(cancellationToken);

            // === 5️⃣ Eski sale itemlarni o‘chirib tashlaymiz ===
            foreach (var oldItem in sale.SaleItems)
            {
                var residue = await context.ProductResidues
                    .FirstOrDefaultAsync(r => r.ProductType.Id == oldItem.ProductTypeId, cancellationToken);

                if (residue is not null)
                    residue.Count += oldItem.Count; // qaytarib qo‘yamiz

                context.SaleItems.Remove(oldItem);
            }

            // === 6️⃣ Yangi itemlar yaratamiz ===
            List<SaleItem> newItems = new();

            foreach (var item in request.SaleItems)
            {
                var residue = productResidues.FirstOrDefault(r => r.ProductType.Id == item.ProductTypeId)
                    ?? throw new NotFoundException(nameof(ProductResidue), nameof(item.ProductTypeId), item.ProductTypeId);

                residue.Count -= item.TypeCount;

                var lastEntry = residue.ProductEntries.LastOrDefault()
                    ?? throw new NotFoundException(nameof(residue.ProductType));

                var saleItem = mapper.Map<SaleItem>(item);
                saleItem.SaleId = sale.Id;
                saleItem.CostPrice = lastEntry.CostPrice * item.Count;
                saleItem.Benifit = (item.Price - lastEntry.CostPrice) * item.Count;

                newItems.Add(saleItem);
            }

            // === 7️⃣ Sale ma’lumotlarini yangilaymiz ===
            sale.Date = request.Date;
            sale.Note = request.Note;
            sale.TotalAmount = request.TotalAmount;
            sale.TotalCount = newItems.Sum(x => x.Count);
            sale.CostPrice = newItems.Sum(x => x.CostPrice);
            sale.BenifitPrice = newItems.Sum(x => x.Benifit);
            sale.UserId = request.UserId;

            // === 8️⃣ Yangilangan itemlarni qo‘shamiz ===
            context.SaleItems.AddRange(newItems);

            // === 9️⃣ Saqlash va commit ===
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

