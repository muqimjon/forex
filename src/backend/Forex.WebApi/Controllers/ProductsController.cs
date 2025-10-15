namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Products.Commands;
using Forex.Application.Features.Products.DTOs;
using Forex.Application.Features.Products.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class ProductsController
    : ReadOnlyController<ProductDto, GetAllProductsQuery, GetProductByIdQuery>
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] ProductRequest request)
        => Ok(new Response
        {
            Data = await Mediator.Send(new CreateProductCommand(
                request.Name,
                request.Code,
                request.Measure,
                request.Photo?.OpenReadStream(),
                request.Photo?.ContentType,
                Path.GetExtension(request.Photo?.FileName)
            ))
        });

    [HttpPut("{id:long}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(long id, [FromForm] ProductRequest request)
        => Ok(new Response
        {
            Data = await Mediator.Send(new UpdateProductCommand(
                id,
                request.Name,
                request.Code,
                request.Measure,
                request.Photo?.OpenReadStream(),
                Path.GetExtension(request.Photo?.FileName ?? string.Empty),
                request.Photo?.ContentType
            ))
        });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteProductCommand(id)) });

    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(ProductFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });
}
