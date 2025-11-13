namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Products.ProductResidues.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class ProductResiduesController : BaseController
{
    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(ProductResidueFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });
}