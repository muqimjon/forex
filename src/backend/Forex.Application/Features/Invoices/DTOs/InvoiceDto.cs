namespace Forex.Application.Features.Invoices.DTOs;

using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.Manufactories.DTOs;
using Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;
using Forex.Application.Features.Users.DTOs;

public sealed record InvoiceDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string? Number { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public bool ViaMiddleman { get; set; }
    public int ContainerCount { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal? TransferFee { get; set; }
    public decimal TotalAmount { get; set; }

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;

    public long ManufactoryId { get; set; }
    public ManufactoryDto Manufactory { get; set; } = default!;

    public long SupplierId { get; set; }
    public UserDto Supplier { get; set; } = default!;

    public long? SenderId { get; set; }
    public UserDto Sender { get; set; } = default!;

    public ICollection<SemiProductEntryDto> SemiProductEntries { get; set; } = default!;
}