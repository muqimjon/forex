namespace Forex.Domain.Entities.Users;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Manufactories;
using Forex.Domain.Entities.Payments;
using Forex.Domain.Entities.Sales;
using Forex.Domain.Enums;

public class User : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public Role Role { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? PasswordHash { get; set; }

    public Account Account { get; set; } = default!;
    public ICollection<Sale> Sales { get; set; } = default!;
    public ICollection<Transaction> Transactions { get; set; } = default!;
    public ICollection<ContainerEntry> ContainerEntries { get; set; } = default!;
}
