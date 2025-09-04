namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class Manufactory : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public ICollection<SemiProduct> SemiProducts { get; set; } = [];
}