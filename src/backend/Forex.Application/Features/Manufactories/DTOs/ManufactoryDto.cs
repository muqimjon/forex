namespace Forex.Application.Features.Manufactories.DTOs;

using Forex.Application.Features.Invoices.DTOs;
using Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;
using Forex.Application.Features.SemiProducts.SemiProductResidues.DTOs;

public sealed class ManufactoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<SemiProductResidueForManufactoryDto> SemiProductResidues { get; set; } = default!;
    public ICollection<SemiProductEntryForManufactoryDto> SemiProductEntries { get; set; } = default!;
    public ICollection<InvoiceForManufactoryDto> Invoices { get; set; } = default!;
}
