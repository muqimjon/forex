namespace Forex.Application.Features.SemiProducts.SemiProductEntries.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Invoices.Invoices.Commands;
using Forex.Application.Features.Products.Products.Commands;
using Forex.Application.Features.Products.ProductTypes.Commands;
using Forex.Application.Features.SemiProducts.SemiProducts.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.SemiProducts;
using Forex.Domain.Enums;
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
            // 🔹 Invoice yaratish
            var invoice = mapper.Map<Invoice>(request.Invoice);
            context.Invoices.Add(invoice);

            foreach (var payment in invoice.Payments)
            {
                if (!request.Invoice.ViaConsolidator && payment.Target == PaymentTarget.Consolidator)
                {
                    invoice.Payments.Remove(payment);
                    continue;
                }

                // Supplier uchun CostPrice
                if (payment.Target == PaymentTarget.Supplier)
                    payment.Amount = request.Invoice.CostPrice;

                // Consolidator uchun ContainerCount * PricePerUnitContainer
                else if (payment.Target == PaymentTarget.Consolidator)
                    payment.Amount = (request.Invoice.ContainerCount ?? 0) * (request.Invoice.PricePerUnitContainer ?? 0);

                var userAccount = await context.UserAccounts.FirstOrDefaultAsync(ua => ua.UserId == payment.UserId && ua.CurrencyId == payment.CurrencyId, ct);
                if (userAccount is null)
                    context.UserAccounts.Add(userAccount = new()
                    {
                        UserId = payment.UserId,
                        CurrencyId = payment.CurrencyId,
                        OpeningBalance = payment.Amount,
                    });

                userAccount.Balance += payment.Amount;
            }

            await RefreshExchangeRate(invoice, ct);
            ValidateCostPrice(request.Invoice.CostPrice);

            var (deliveryRatio, transferRatio) = CalculateRatios(request.Invoice);

            // 🔹 Manufactory
            var manufactory = await GetOrCreateManufactoryAsync(ct);
            invoice.Manufactory = manufactory;

            var defaultMeasure = await GetDefaultMeasureAsync(ct);

            ProcessProducts(
                request.Products,
                invoice,
                manufactory,
                defaultMeasure,
                deliveryRatio,
                transferRatio);

            ProcessIndependentSemiProducts(
                request.SemiProducts,
                invoice,
                manufactory,
                deliveryRatio,
                transferRatio);

            await context.CommitTransactionAsync(ct);
            return invoice.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task RefreshExchangeRate(Invoice invoice, CancellationToken ct)
    {
        foreach (var payment in invoice.Payments)
        {
            var currency = await context.Currencies
                .FirstOrDefaultAsync(c => c.Id == payment.CurrencyId, ct)
                ?? throw new NotFoundException(nameof(Currency), nameof(payment.CurrencyId), payment.CurrencyId);

            if (currency.NormalizedName != "So'm".ToNormalized() && payment.ExchangeRate != 0)
                currency.ExchangeRate = payment.ExchangeRate;
        }
    }

    private async Task<Manufactory> GetOrCreateManufactoryAsync(CancellationToken ct)
    {
        var manufactory = await context.Manufactories.FirstOrDefaultAsync(ct);

        if (manufactory is null)
        {
            manufactory = new Manufactory { Name = "Default" };
            context.Manufactories.Add(manufactory);
        }
        manufactory.SemiProductEntries = [];
        manufactory.SemiProductResidues = [];

        return manufactory;
    }

    private async Task<UnitMeasure> GetDefaultMeasureAsync(CancellationToken ct)
    {
        return await context.UnitMeasures
            .FirstOrDefaultAsync(um => um.NormalizedName == "Dona".ToNormalized(), ct)
            ?? await context.UnitMeasures.FirstOrDefaultAsync(um => um.IsDefault, ct)
            ?? await context.UnitMeasures.FirstOrDefaultAsync(ct)
            ?? throw new ForbiddenException("O'lchov birliklari mavjud emas");
    }

    private static void ValidateCostPrice(decimal costPrice)
    {
        if (costPrice == 0)
            throw new ForbiddenException("Umumiy tannarx (CostPrice) 0 bo'lishi mumkin emas.");
    }

    private static (decimal deliveryRatio, decimal transferRatio) CalculateRatios(InvoiceCommand invoice)
    {
        var deliveryRatio = invoice.CostDelivery / invoice.CostPrice;

        var transferRatio = (invoice.ConsolidatorFee ?? 0) / invoice.CostPrice;

        return (deliveryRatio, transferRatio);
    }

    // 🔹 Qolgan product va semi-product processing metodlari o‘zgarmaydi
    private void ProcessProducts(
        IEnumerable<ProductCommand> productCommands,
        Invoice invoice,
        Manufactory manufactory,
        UnitMeasure defaultMeasure,
        decimal deliveryRatio,
        decimal transferRatio)
    {
        foreach (var pCmd in productCommands)
        {
            var product = CreateProduct(pCmd, defaultMeasure);

            foreach (var typeCmd in pCmd.ProductTypes)
            {
                var productType = CreateProductType(typeCmd, product);

                foreach (var itemCmd in typeCmd.ProductTypeItems)
                {
                    var semi = mapper.Map<SemiProduct>(itemCmd.SemiProduct);
                    var (costPrice, costDelivery, transferFee) = CalculateSemiProductCosts(
                        itemCmd.SemiProduct.CostPrice,
                        itemCmd.Quantity,
                        deliveryRatio,
                        transferRatio);

                    AddSemiProductEntry(manufactory, semi, itemCmd.SemiProduct.Quantity, invoice, costPrice, costDelivery, transferFee);
                    AddSemiProductResidue(manufactory, semi, itemCmd.SemiProduct.Quantity);
                    AddProductTypeItem(productType, semi, itemCmd.Quantity);
                }
            }
        }
    }

    private void ProcessIndependentSemiProducts(
        IEnumerable<SemiProductCommand> semiProductCommands,
        Invoice invoice,
        Manufactory manufactory,
        decimal deliveryRatio,
        decimal transferRatio)
    {
        foreach (var cmd in semiProductCommands)
        {
            var semi = mapper.Map<SemiProduct>(cmd);
            context.SemiProducts.Add(semi);

            var costDelivery = cmd.CostPrice * deliveryRatio;
            var transferFee = cmd.CostPrice * transferRatio;

            AddSemiProductEntry(manufactory, semi, cmd.Quantity, invoice, cmd.CostPrice, costDelivery, transferFee);
            AddSemiProductResidue(manufactory, semi, cmd.Quantity);
        }
    }

    private Product CreateProduct(ProductCommand pCmd, UnitMeasure defaultMeasure)
    {
        var product = mapper.Map<Product>(pCmd);
        product.UnitMeasure = defaultMeasure;
        product.ProductTypes.Clear();
        context.Products.Add(product);
        return product;
    }

    private ProductType CreateProductType(ProductTypeCommand typeCmd, Product product)
    {
        var productType = mapper.Map<ProductType>(typeCmd);
        productType.ProductTypeItems.Clear();
        product.ProductTypes.Add(productType);
        return productType;
    }

    private static (decimal costPrice, decimal costDelivery, decimal transferFee) CalculateSemiProductCosts(
        decimal totalCost,
        decimal quantity,
        decimal deliveryRatio,
        decimal transferRatio)
    {
        var costPrice = totalCost / quantity;
        var costDelivery = costPrice * deliveryRatio;
        var transferFee = costPrice * transferRatio;
        return (costPrice, costDelivery, transferFee);
    }

    private static void AddSemiProductEntry(
        Manufactory manufactory,
        SemiProduct semi,
        decimal quantity,
        Invoice invoice,
        decimal costPrice,
        decimal costDelivery,
        decimal transferFee)
    {
        manufactory.SemiProductEntries.Add(new SemiProductEntry
        {
            SemiProduct = semi,
            Quantity = quantity,
            Invoice = invoice,
            CostPrice = costPrice,
            CostDelivery = costDelivery,
            ConsolidatorFee = transferFee
        });
    }

    private static void AddSemiProductResidue(Manufactory manufactory, SemiProduct semi, decimal quantity)
    {
        manufactory.SemiProductResidues.Add(new SemiProductResidue
        {
            SemiProduct = semi,
            Quantity = quantity
        });
    }

    private static void AddProductTypeItem(ProductType productType, SemiProduct semi, decimal quantity)
    {
        productType.ProductTypeItems.Add(new ProductTypeItem
        {
            SemiProduct = semi,
            Quantity = quantity
        });
    }
}
