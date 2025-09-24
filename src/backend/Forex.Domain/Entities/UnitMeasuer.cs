namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class UnitMeasuer : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}