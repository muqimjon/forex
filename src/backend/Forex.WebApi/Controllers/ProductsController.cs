namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Products.Products.Commands;
using Forex.Application.Features.Products.Products.DTOs;
using Forex.Application.Features.Products.Products.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class ProductsController
    : ReadOnlyController<ProductDto, GetAllProductsQuery, GetProductByIdQuery>
{
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteProductCommand(id)) });

    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(ProductFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });
}
