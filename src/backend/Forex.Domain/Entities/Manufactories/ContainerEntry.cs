namespace Forex.Domain.Entities.Manufactories;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Users;

public class ContainerEntry : Auditable
{
    public long SenderId { get; set; }
    public long InvoceId { get; set; }
    public long Count { get; set; }
    public decimal Price { get; set; }

    public User Sender { get; set; } = default!;
    public Invoice Invoce { get; set; } = default!;
}