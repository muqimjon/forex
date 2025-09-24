namespace Forex.Application.Features.SemiProductEntries.DTOs;

using Microsoft.AspNetCore.Http;

public class SemiProductCommand
{
    public string? Name { get; set; }
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public int Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public IFormFile File { get; set; } = default!;
}