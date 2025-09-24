namespace Forex.Application.Features.SemiProductEntries.Commands;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.SemiProductEntries.DTOs;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Manufactories;
using Forex.Domain.Entities.Shops;
using Forex.Domain.Entities.Users;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateSemiProductIntakeCommand(
    InvoiceCommand Invoice,
    IEnumerable<SemiProductCommand> SemiProducts,
    IEnumerable<ProductCommand> Products
) : IRequest<long>;

public class CreateSemiProductIntakeCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IFileStorageService fileStorage)
    : IRequestHandler<CreateSemiProductIntakeCommand, long>
{
    public async Task<long> Handle(CreateSemiProductIntakeCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            await HandleMiddlemanTransferAsync(request.Invoice, ct);
            await EnsureSupplierExistsAsync(request.Invoice, ct);

            var invoice = mapper.Map<Invoice>(request.Invoice);
            context.Invoices.Add(invoice);

            var semiProducts = await AddOrUpdateSemiProductsAsync(request.SemiProducts, invoice, ct);
            await AddOrUpdateProductsAsync(request.Products, semiProducts, ct);

            await context.CommitTransactionAsync(ct);
            return invoice.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task HandleMiddlemanTransferAsync(InvoiceCommand invoice, CancellationToken ct)
    {
        if (!invoice.ViaMiddleman) return;

        var sender = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == invoice.Sender!.Id, ct);

        if (sender is null) return;

        var account = sender.Accounts.FirstOrDefault(a => a.Id == invoice.CurrencyId);
        if (account is null)
        {
            account = new UserAccount
            {
                OpeningBalance = (decimal)invoice.TransferFee!,
                User = sender
            };
            context.Accounts.Add(account);
        }

        account.Balance += (decimal)invoice.TransferFee!;
    }

    private async Task EnsureSupplierExistsAsync(InvoiceCommand invoice, CancellationToken ct)
    {
        var user = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == invoice.Supplier.Id, ct);

        if (user is null)
        {
            user = mapper.Map<User>(invoice.Supplier);
            user.Role = Role.Taminotchi;

            user.Accounts.Add(new UserAccount
            {
                Balance = (decimal)invoice.TransferFee!,
                OpeningBalance = (decimal)invoice.TransferFee!
            });

            context.Users.Add(user);
        }
        else
        {
            var account = user.Accounts.FirstOrDefault(a => a.Id == invoice.CurrencyId);
            if (account is null)
            {
                account = new UserAccount
                {
                    OpeningBalance = (decimal)invoice.TransferFee!,
                    User = user
                };
                context.Accounts.Add(account);
            }

            account.Balance += (decimal)invoice.TransferFee!;
        }
    }

    private async Task<IEnumerable<SemiProduct>> AddOrUpdateSemiProductsAsync(IEnumerable<SemiProductCommand> commands, Invoice invoice, CancellationToken ct)
    {
        var manufactory = await context.Manufactories
            .Include(m => m.SemiProductResidues)
            .Include(m => m.SemiProductEntries)
            .FirstOrDefaultAsync(m => m.Id == invoice.ManufactoryId, ct);

        var codes = commands.Select(x => x.Code).ToList();
        var existingSemiProducts = await context.SemiProducts
            .Where(sp => codes.Contains(sp.Code))
            .ToListAsync(ct);

        var result = new List<SemiProduct>();
        const decimal deliveryPercent = 0.6m;
        const decimal transferPercent = 0.4m;

        foreach (var cmd in commands)
        {
            var semi = existingSemiProducts.FirstOrDefault(sp => sp.Code == cmd.Code);

            if (semi is null)
            {
                semi = mapper.Map<SemiProduct>(cmd);

                if (cmd.File is not null)
                    semi.PhotoPath = await fileStorage.UploadAsync(cmd.File, ct);

                context.SemiProducts.Add(semi);
            }

            context.SemiProductEntries.Add(new SemiProductEntry
            {
                SemiProduct = semi,
                Quantity = cmd.Quantity,
                Manufactory = manufactory!,
                Invoce = invoice,
                CostPrice = cmd.CostPrice,
                CostDelivery = cmd.CostPrice * deliveryPercent,
                TransferFee = cmd.CostPrice * transferPercent
            });

            var residue = manufactory!.SemiProductResidues.FirstOrDefault(r => r.SemiProductId == semi.Id);
            if (residue is null)
            {
                context.SemiProductResidues.Add(new SemiProductResidue
                {
                    SemiProduct = semi,
                    Manufactory = manufactory,
                    Quantity = cmd.Quantity
                });
            }
            else
            {
                residue.Quantity += cmd.Quantity;
            }

            result.Add(semi);
        }

        return result;
    }

    private async Task AddOrUpdateProductsAsync(IEnumerable<ProductCommand> commands, IEnumerable<SemiProduct> semiProducts, CancellationToken ct)
    {
        var codes = commands.Select(p => p.Code).ToList();
        var existingProducts = await context.Products
            .Include(p => p.ProductTypes)
                .ThenInclude(pt => pt.Items)
            .Where(p => codes.Contains(p.Code))
            .ToListAsync(ct);

        foreach (var cmd in commands)
        {
            var product = existingProducts.FirstOrDefault(p => p.Code == cmd.Code);

            if (product is null)
            {
                product = mapper.Map<Product>(cmd);

                if (cmd.File is not null)
                    product.PhotoPath = await fileStorage.UploadAsync(cmd.File, ct);

                foreach (var type in product.ProductTypes)
                    foreach (var item in type.Items)
                        item.SemiProduct = semiProducts.First(sp => sp.Code == item.SemiProductCode);

                context.Products.Add(product);
            }
            else
            {
                product.Name = cmd.Name;
                product.MeasureId = cmd.MeasureId;

                if (cmd.File is not null)
                {
                    if (!string.IsNullOrEmpty(product.PhotoPath))
                        await fileStorage.DeleteAsync(product.PhotoPath, ct);

                    using var stream = cmd.File.OpenReadStream();
                    product.PhotoPath = await fileStorage.UploadAsync(stream, cmd.File.FileName, cmd.File.ContentType, ct);
                }

                foreach (var typeCmd in cmd.Types)
                {
                    var existingType = product.ProductTypes.FirstOrDefault(t => t.Type == typeCmd.Type);

                    if (existingType is null)
                    {
                        var newType = new ProductType
                        {
                            Type = typeCmd.Type,
                            Items = [.. typeCmd.Items.Select(i => new ProductTypeItem
                            {
                                SemiProductCode = i.SemiProductCode,
                                Quantity = i.Quantity,
                                SemiProduct = semiProducts.First(sp => sp.Code == i.SemiProductCode)
                            })]
                        };

                        product.ProductTypes.Add(newType);
                    }
                    else
                    {
                        existingType.Items.Clear();

                        foreach (var itemCmd in typeCmd.Items)
                        {
                            existingType.Items.Add(new ProductTypeItem
                            {
                                SemiProductCode = itemCmd.SemiProductCode,
                                Quantity = itemCmd.Quantity,
                                SemiProduct = semiProducts.First(sp => sp.Code == itemCmd.SemiProductCode)
                            });
                        }
                    }
                }
            }
        }
    }
}
