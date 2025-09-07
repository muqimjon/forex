namespace Forex.Application.Features.SemiProductEntries.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.SemiProductEntries.DTOs;
using Forex.Domain.Entities.Manufactories;
using Forex.Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateSemiProductEntryCommand(
    long SenderId,
    decimal CostDelivery,
    decimal TransferFee,
    DateTime EntryDate,
    List<SemiProductEntryDto> Entries)
    : IRequest<long>;


public sealed class CreateSemiProductEntryCommandHandler(
    IAppDbContext context)
    : IRequestHandler<CreateSemiProductEntryCommand, long>
{
    public async Task<long> Handle(CreateSemiProductEntryCommand request, CancellationToken cancellationToken)
    {
        var sender = await context.Users
            .Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.Id == request.SenderId, cancellationToken)
            ?? throw new NotFoundException("Sender not found");

        var manufactory = await context.Manufactories.FirstAsync(cancellationToken);

        var invoice = CreateInvoice(request);
        context.Invoices.Add(invoice);

        var containerEntry = CreateContainerEntry(sender, invoice);
        context.ContainerEntries.Add(containerEntry);

        var semiProductEntries = new List<SemiProductEntry>();
        var totalCost = request.Entries.Sum(e => e.CostPrice * e.Quantity);

        foreach (var dto in request.Entries)
        {
            var semiProduct = await GetOrCreateSemiProduct(dto, cancellationToken);
            var costRatio = (dto.CostPrice * dto.Quantity) / totalCost;

            var entry = new SemiProductEntry
            {
                SemiProductId = semiProduct.Id,
                Invoce = invoice,
                Manufactory = manufactory,
                Quantity = dto.Quantity,
                CostPrice = dto.CostPrice,
                CostDelivery = request.CostDelivery * costRatio,
                TransferFee = request.TransferFee * costRatio
            };

            semiProductEntries.Add(entry);
            await UpdateResidue(semiProduct.Id, manufactory.Id, dto.Quantity, cancellationToken);
        }

        context.SemiProductEntries.AddRange(semiProductEntries);

        sender.Account.Balance += 3000;

        await context.SaveAsync(cancellationToken);
        return invoice.Id;
    }

    private static Invoice CreateInvoice(CreateSemiProductEntryCommand request) =>
        new()
        {
            EntryDate = request.EntryDate,
            CostPrice = request.Entries.Sum(e => e.CostPrice * e.Quantity),
            CostDelivery = request.CostDelivery,
            TransferFee = request.TransferFee
        };

    private static ContainerEntry CreateContainerEntry(User sender, Invoice invoice) =>
        new()
        {
            Sender = sender,
            Invoce = invoice,
            Count = 1,
            Price = 3000
        };

    private async Task<SemiProduct> GetOrCreateSemiProduct(SemiProductEntryDto dto, CancellationToken ct)
    {
        var normalizedName = dto.Name.ToLower().Trim();
        var existing = await context.SemiProducts
            .FirstOrDefaultAsync(p => p.NormalizedName == normalizedName, ct);

        if (existing is not null) return existing;

        var newProduct = new SemiProduct
        {
            Name = dto.Name,
            NormalizedName = normalizedName,
            Code = dto.Code,
            Measure = dto.Measure
        };

        context.SemiProducts.Add(newProduct);
        return newProduct;
    }

    private async Task UpdateResidue(long semiProductId, long manufactoryId, decimal quantity, CancellationToken ct)
    {
        var residue = await context.SemiProductResidues
            .FirstOrDefaultAsync(r => r.SemiProductId == semiProductId && r.ManufactoryId == manufactoryId, ct);

        if (residue is null)
        {
            context.SemiProductResidues.Add(new SemiProductResidue
            {
                SemiProductId = semiProductId,
                ManufactoryId = manufactoryId,
                Quantity = quantity
            });
        }
        else
        {
            residue.Quantity += quantity;
        }
    }
}
