namespace Forex.Application.Features.SemiProductEntries.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.SemiProductEntries.DTOs;
using Forex.Application.Features.Users.DTOs;
using Forex.Domain.Entities.Manufactories;
using Forex.Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

public record CreateSemiProductIntakeCommand(
    Invoice Invoice,
    UserDto Supplier,
    long ViaMiddlemanId,
    long ManufactoryId,
    DateTime EntryDate,
    decimal TransferFeePerContainer,
    IEnumerable<ContainerDto> Containers,
    IEnumerable<ItemDto> Items
) : IRequest<long>;

public class CreateSemiProductIntakeCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IFileStorageService fileStorage)
    : IRequestHandler<CreateSemiProductIntakeCommand, long>
{
    public async Task<long> Handle(CreateSemiProductIntakeCommand request, CancellationToken ct)
    {
        if (request.Invoice.ViaMiddleman)
        {
            var user = new User();
            if (request.Supplier.Id <= 0)
            {
                user = mapper.Map<User>(request.Supplier);
                await context.Users.AddAsync(user);
                await context.SaveAsync(ct);
            }
            else
            {
                user = await context.Users
                    .Include(u => u.Account)
                    .FirstOrDefaultAsync(u => u.Id == request.Supplier.Id, ct);
            }


            var manufactory = await context.Manufactories
                .FirstOrDefaultAsync(m => m.Id == request.ManufactoryId, ct)
                ?? throw new NotFoundException(nameof(Manufactory), nameof(request.ManufactoryId), request.ManufactoryId);

            var totalContainerCount = request.Containers.Sum(c => c.Count);
            var invoiceTransferFeeTotal = request.Items.Sum(i => i.TransferFee);

            if (invoiceTransferFeeTotal == 0 && request.TransferFeePerContainer > 0)
                invoiceTransferFeeTotal = request.TransferFeePerContainer * totalContainerCount;

            var invoice = new Invoice
            {
                EntryDate = request.EntryDate,
                CostPrice = request.Items.Sum(i => i.CostPrice),
                CostDelivery = request.Items.Sum(i => i.CostDelivery),
                TransferFee = invoiceTransferFeeTotal
            };

            await context.BeginTransactionAsync(ct);

            context.Invoices.Add(invoice);

            foreach (var c in request.Containers.Where(c => c.Count > 0))
            {
                context.ContainerEntries.Add(new ContainerEntry
                {
                    SenderId = user.Id,
                    InvoceId = invoice.Id,
                    Count = c.Count,
                    Price = c.Price
                });
            }

            if (request.TransferFeePerContainer > 0)
                user.Account.Balance += request.TransferFeePerContainer * totalContainerCount;

            foreach (var item in request.Items)
            {
                var semiProductId = await EnsureSemiProductAsync(item, ct);

                var entry = mapper.Map<SemiProductEntry>(item);
                entry.SemiProductId = semiProductId;
                entry.InvoceId = invoice.Id;
                entry.ManufactoryId = manufactory.Id;

                context.SemiProductEntries.Add(entry);

                await UpsertResidueAsync(semiProductId, manufactory.Id, item.Quantity, ct);
            }

            await context.CommitTransactionAsync(ct);
            return invoice.Id;
        }
    }

    private async Task<long> EnsureSemiProductAsync(ItemDto item, CancellationToken ct)
    {
        if (item.SemiProductId is long id && id > 0)
        {
            var exists = await context.SemiProducts.AnyAsync(sp => sp.Id == id, ct);
            if (!exists) throw new NotFoundException(nameof(SemiProduct), nameof(id), id);
            return id;
        }

        if (item.Code is int code)
        {
            var found = await context.SemiProducts.FirstOrDefaultAsync(sp => sp.Code == code, ct);
            if (found is not null) return found.Id;
        }

        if (!string.IsNullOrWhiteSpace(item.Name))
        {
            var normalized = item.Name.Trim().ToUpperInvariant();
            var found = await context.SemiProducts.FirstOrDefaultAsync(sp =>
                sp.NormalizedName == normalized &&
                (string.IsNullOrEmpty(item.Measure) || sp.Measure == item.Measure), ct);

            if (found is not null) return found.Id;
        }

        var semi = mapper.Map<SemiProduct>(item);

        if (item.Photo is not null)
        {
            var fileName = $"{Guid.NewGuid():N}{item.Extension}";
            semi.PhotoPath = await fileStorage.UploadAsync(
                item.Photo,
                fileName,
                item.ContentType ?? "application/octet-stream",
                ct);
        }

        context.SemiProducts.Add(semi);
        await context.SaveAsync(ct);

        return semi.Id;
    }

    private async Task UpsertResidueAsync(long semiProductId, long manufactoryId, decimal quantity, CancellationToken ct)
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
