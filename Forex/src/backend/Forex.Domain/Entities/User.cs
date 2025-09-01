namespace Forex.Domain.Entities;

using Forex.Domain.Enums;

public class User : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName {  get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Role Role { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
