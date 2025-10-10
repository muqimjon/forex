namespace Forex.Application.Features.Sales.DTOs;
using System;
using System.Collections.Generic;

public record SaleDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<SaleItemDto> Items { get; set; } = new();
}