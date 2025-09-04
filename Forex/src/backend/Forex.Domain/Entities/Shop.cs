namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class Shop : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
}