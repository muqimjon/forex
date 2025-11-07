namespace Forex.Application.Features.SemiProducts.SemiProductEntries.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Invoices.Commands;
using Forex.Application.Features.Products.Products.Commands;
using Forex.Application.Features.SemiProducts.SemiProducts.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.SemiProducts;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateSemiProductIntakeCommand(
    InvoiceCommand Invoice,
    List<SemiProductCommand> SemiProducts,
    List<ProductCommand> Products
) : IRequest<long>;

public class CreateSemiProductIntakeCommandHandler(
    IAppDbContext context,
    IMapper mapper
) : IRequestHandler<CreateSemiProductIntakeCommand, long>
{
    public async Task<long> Handle(CreateSemiProductIntakeCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            await HandleMiddlemanTransferAsync(request.Invoice, ct);
            await EnsureSupplierExistsAsync(request.Invoice, ct);

            // 🔹 Invoice ni saqlaymiz
            var invoice = mapper.Map<Invoice>(request.Invoice);
            context.Invoices.Add(invoice);

            // 🔹 Foiz nisbatlarini aniqlaymiz
            if (request.Invoice.CostPrice == 0)
                throw new ForbiddenException("Umumiy tannarx (CostPrice) 0 bo‘lishi mumkin emas.");

            var deliveryRatio = request.Invoice.CostDelivery / request.Invoice.CostPrice;
            var transferRatio = (request.Invoice.TransferFee ?? 0) / request.Invoice.CostPrice;

            // 🔹 Avvalo Productlar va ularga bog‘langan SemiProductlarni saqlaymiz
            await AddProductsAsync(request.Products, invoice, deliveryRatio, transferRatio, ct);

            // 🔹 Keyin mustaqil SemiProductlarni (productga bog‘liq bo‘lmagan)
            await AddIndependentSemiProductsAsync(request.SemiProducts, invoice, deliveryRatio, transferRatio, ct);

            await context.CommitTransactionAsync(ct);
            return invoice.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    // --- Middleman Transfer ---
    private async Task HandleMiddlemanTransferAsync(InvoiceCommand invoice, CancellationToken ct)
    {
        if (!invoice.ViaMiddleman) return;

        var user = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == invoice.SenderId, ct)
            ?? throw new NotFoundException(nameof(User), nameof(invoice.SenderId), invoice.SenderId!);

        var account = user.Accounts.FirstOrDefault(a => a.CurrencyId == invoice.CurrencyId);
        if (account is null)
        {
            account = new UserAccount
            {
                OpeningBalance = (decimal)invoice.TransferFee!,
                User = user,
                CurrencyId = invoice.CurrencyId
            };

            context.Accounts.Add(account);
        }

        account.Balance += (decimal)invoice.TransferFee!;
    }

    private async Task EnsureSupplierExistsAsync(InvoiceCommand invoice, CancellationToken ct)
    {
        var user = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == invoice.SupplierId, ct)
            ?? throw new NotFoundException(nameof(User), nameof(invoice.SupplierId), invoice.SupplierId);

        var account = user.Accounts.FirstOrDefault(a => a.CurrencyId == invoice.CurrencyId);
        if (account is null)
        {
            account = new UserAccount
            {
                OpeningBalance = invoice.CostPrice,
                User = user,
                CurrencyId = invoice.CurrencyId
            };
            context.Accounts.Add(account);
        }

        account.Balance += invoice.CostPrice!;
    }

    private async Task AddProductsAsync(
        IEnumerable<ProductCommand> productCommands,
        Invoice invoice,
        decimal deliveryRatio,
        decimal transferRatio,
        CancellationToken ct)
    {
        var manufactory = await context.Manufactories
            .FirstOrDefaultAsync(ct);

        if (manufactory is null)
            context.Manufactories.Add(manufactory = new() { Name = "Default" });

        var defaultMeasure = await context.UnitMeasures
            .FirstOrDefaultAsync(um => um.NormalizedName == "Dona".ToNormalized(), ct)
            ?? await context.UnitMeasures.FirstOrDefaultAsync(um => um.IsDefault, ct)
            ?? await context.UnitMeasures.FirstOrDefaultAsync(ct)
            ?? throw new ForbiddenException("O'lchov birliklari mavjud emas");

        foreach (var pCmd in productCommands)
        {
            var product = mapper.Map<Product>(pCmd);
            product.UnitMeasure = defaultMeasure;
            product.ProductTypes.Clear();
            context.Products.Add(product);

            foreach (var typeCmd in pCmd.ProductTypes)
            {
                var productType = mapper.Map<ProductType>(typeCmd);
                productType.ProductTypeItems.Clear();
                product.ProductTypes.Add(productType);

                foreach (var itemCmd in typeCmd.ProductTypeItems)
                {
                    var semi = mapper.Map<SemiProduct>(itemCmd.SemiProduct);

                    var semiCost = itemCmd.SemiProduct.CostPrice / itemCmd.Quantity;
                    var costDelivery = semiCost * deliveryRatio;
                    var transferFee = semiCost * transferRatio;

                    context.SemiProductEntries.Add(new SemiProductEntry
                    {
                        SemiProduct = semi,
                        Quantity = itemCmd.SemiProduct.Quantity,
                        Manufactory = manufactory!,
                        Invoice = invoice,
                        CostPrice = semiCost,
                        CostDelivery = costDelivery,
                        TransferFee = transferFee
                    });

                    context.SemiProductResidues.Add(new SemiProductResidue
                    {
                        SemiProduct = semi,
                        Manufactory = manufactory,
                        Quantity = itemCmd.SemiProduct.Quantity
                    });

                    productType.ProductTypeItems.Add(new ProductTypeItem
                    {
                        SemiProduct = semi,
                        Quantity = itemCmd.Quantity
                    });
                }
            }
        }
    }


    // --- 2️⃣ Mustaqil ProductTypeId’lar ---
    private async Task AddIndependentSemiProductsAsync(
    IEnumerable<SemiProductCommand> semiProductCommands,
    Invoice invoice,
    decimal deliveryRatio,
    decimal transferRatio,
    CancellationToken ct)
    {
        var manufactory = await context.Manufactories.FirstOrDefaultAsync(ct);

        if (manufactory is null)
            context.Manufactories.Add(manufactory = new() { Name = "Default" });

        invoice.Manufactory = manufactory;

        foreach (var cmd in semiProductCommands)
        {
            var semi = mapper.Map<SemiProduct>(cmd);
            context.SemiProducts.Add(semi);

            var costDelivery = cmd.CostPrice * deliveryRatio;
            var transferFee = cmd.CostPrice * transferRatio;

            context.SemiProductEntries.Add(new SemiProductEntry
            {
                SemiProduct = semi,
                Quantity = cmd.Quantity,
                Manufactory = manufactory!,
                Invoice = invoice,
                CostPrice = cmd.CostPrice,
                CostDelivery = costDelivery,
                TransferFee = transferFee
            });

            context.SemiProductResidues.Add(new SemiProductResidue
            {
                SemiProduct = semi,
                Manufactory = manufactory,
                Quantity = cmd.Quantity
            });
        }
    }
}
