namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class ContainerEntry : Auditable
{
    public int UserId { get; set; }
    public int InvoceId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }

    public User User { get; set; } = default!;
    public Invoice Invoce { get; set; } = default!;
}