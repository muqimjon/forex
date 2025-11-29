namespace Forex.Application.Features.Sales.DTOs;

using Forex.Application.Features.OperationRecords.DTOs;
using Forex.Application.Features.Sales.SaleItems.DTOs;
using Forex.Application.Features.Users.DTOs;

public sealed record SaleDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public decimal CostPrice { get; set; }      // 1 ta savdoda umumiy tannarxi
    public decimal BenifitPrice { get; set; }    // 1 ta savdoda umumiy foydasi
    public int TotalCount { get; set; }       // 1 ta savdoda jami necha dona sotildi
    public decimal TotalAmount { get; set; }   // 1 ta savdoda jami summa
    public string? Note { get; set; }

    public long OperationRecordId { get; set; }
    public OperationRecordForSaleDto OperationRecord { get; set; } = default!;

    public long CustomerId { get; set; }
    public UserForSaleDto Customer { get; set; } = default!;

    public ICollection<SaleItemForSaleDto> SaleItems { get; set; } = default!;
}
