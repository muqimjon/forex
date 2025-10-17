namespace Forex.Application.Features.Manufactories.DTOs;

using Forex.Application.Features.Invoices.DTOs;
using Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;

public sealed class ManufactoryForSemiProductResidueDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<SemiProductEntryForManufactoryDto> SemiProductEntries { get; set; } = default!;
    public ICollection<InvoiceForManufactoryDto> Invoices { get; set; } = default!;
}