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

public record CreateSaleCommand(
    DateTime Date,
    long UserId,
    int TotalCount,  // jami necha dona sotildi
    decimal TotalAmount, // jami summa
    string? Note,
    List<SaleItemCreateDto> SaleItems

    ) : IRequest<long>;

public class CreateSaleCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateSaleCommand, long>
{
    public async Task<long> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await context.Users
                .Include(a => a.Accounts)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), nameof(request.UserId), request.UserId);

            var userAccount = user.Accounts.FirstOrDefault();
            if (userAccount is null)
                throw new NotFoundException(nameof(UserAccount), nameof(user.Id), user.Id);

            userAccount.Balance -= request.TotalAmount;


            var productTypeIds = request.SaleItems.Select(i => i.ProductTypeId).ToList();

            var productResidues = await context.ProductResidues
                .Include(p => p.ProductEntries)
                .Where(p => productTypeIds.Contains(p.ProductType.Id))
                .ToListAsync(cancellationToken);


            // 🔹 Avval Sale yaratiladi
            var sale = mapper.Map<Sale>(request);
            context.Sales.Add(sale);
            await context.SaveAsync(cancellationToken); // shu joyda sale.Id hosil bo‘ladi

            List<SaleItem> saleItems = [];

            foreach (var item in request.SaleItems)
            {
                var residue = productResidues.FirstOrDefault(r => r.ProductType.Id == item.ProductTypeId)
                    ?? throw new NotFoundException(nameof(ProductResidue), nameof(item.ProductTypeId), item.ProductTypeId);

                residue.ProductType.Count -= item.TypeCount;

                var saleItem = mapper.Map<SaleItem>(item);
                var lastEntry = residue.ProductEntries.LastOrDefault()
                        ?? throw new NotFoundException(nameof(residue.ProductType.Id), nameof(residue.ProductType.Id), residue.ProductType.Id);

                var costPrice = lastEntry.CostPrice;

                saleItem.CostPrice = costPrice * item.Count;
                saleItem.Benifit = (item.Price - costPrice) * item.Count;
                saleItem.SaleId = sale.Id; // 👈 shu joyda bog‘ladik

                saleItems.Add(saleItem);
            }

            // umumiy summalar hisoblanadi
            sale.CostPrice = saleItems.Sum(a => a.CostPrice);
            sale.BenifitPrice = saleItems.Sum(s => s.Benifit);

            context.SaleItems.AddRange(saleItems);

            await context.CommitTransactionAsync(cancellationToken);
            return sale.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;

        }
    }
}
