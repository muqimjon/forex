namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Products.Commands;
using Forex.Application.Features.Products.Queries;
using Forex.WebApi.Models.Commons;
using Forex.WebApi.Models.Products;
using Microsoft.AspNetCore.Mvc;

public class ProductsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateProductRequest request)
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
    public async Task<IActionResult> Update(long id, [FromForm] UpdateProductRequest request)
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

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
        => Ok(new Response { Data = await Mediator.Send(new GetProductByIdQuery(id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllProductsQuery()) });
}