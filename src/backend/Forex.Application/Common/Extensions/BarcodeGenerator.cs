namespace Forex.Application.Common.Extensions;

using Forex.Domain.Entities.Products;

public static class BarcodeGenerator
{
    public static string Prefix { get; set; } = "FRX";

    public static void EnsureBarcodes(ProductType productType)
    {
        if (productType.Id <= 0) return;

        if (string.IsNullOrWhiteSpace(productType.QopBarcode))
            productType.QopBarcode = Build("Q", productType.Id);

        if (string.IsNullOrWhiteSpace(productType.PackBarcode))
            productType.PackBarcode = Build("T", productType.Id);
    }

    private static string Build(string unit, long id)
    {
        var prefix = string.IsNullOrWhiteSpace(Prefix) ? string.Empty : Prefix.Trim() + "-";
        return $"{prefix}{unit}-{id:D6}";
    }
}
