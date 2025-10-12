namespace Forex.ClientService.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record ProductRequest
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
}
