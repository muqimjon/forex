namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiProductResidues

{
    [Post("/product-residues/filter")]
    Task<Response<List<ProductResidueResponse>>> Filter(FilteringRequest request);
}
