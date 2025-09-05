namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class ContainerEntry : Auditable
{
    public long UserId { get; set; }
    public long InvoceId { get; set; }
    public long Count { get; set; }
    public decimal Price { get; set; }

    public User User { get; set; } = default!;
    public Invoice Invoce { get; set; } = default!;
}