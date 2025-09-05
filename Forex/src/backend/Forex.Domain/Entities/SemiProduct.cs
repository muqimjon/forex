namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class SemiProduct : Auditable
{
    public long ManufactoryId { get; set; }
    public string? Name { get; set; }
    public string NormalizedName { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }

    public Manufactory Manufactory { get; set; } = default!;
}