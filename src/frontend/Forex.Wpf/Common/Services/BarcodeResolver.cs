namespace Forex.Wpf.Common.Services;

using Forex.Wpf.ViewModels;

public enum BarcodeUnit { Qop, Pack }

public sealed record BarcodeMatch(ProductViewModel Product, ProductTypeViewModel ProductType, BarcodeUnit Unit);

public static class BarcodeResolver
{
    public static BarcodeMatch? Resolve(IEnumerable<ProductViewModel> products, string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        code = code.Trim();

        foreach (var product in products)
        {
            if (product.ProductTypes is null) continue;

            foreach (var type in product.ProductTypes)
            {
                if (string.Equals(type.QopBarcode, code, StringComparison.OrdinalIgnoreCase))
                    return new BarcodeMatch(product, type, BarcodeUnit.Qop);

                if (string.Equals(type.PackBarcode, code, StringComparison.OrdinalIgnoreCase))
                    return new BarcodeMatch(product, type, BarcodeUnit.Pack);
            }
        }

        return null;
    }
}
