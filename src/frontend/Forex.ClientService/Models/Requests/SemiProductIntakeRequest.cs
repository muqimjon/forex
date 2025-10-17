namespace Forex.ClientService.Models.Requests;

using System.Collections.Generic;

public sealed record SemiProductIntakeRequest
{
    public InvoiceRequest Invoice { get; set; } = default!;
    public ICollection<SemiProductRequest> SemiProducts { get; set; } = default!;
    public ICollection<ProductRequest> Products { get; set; } = default!;
}