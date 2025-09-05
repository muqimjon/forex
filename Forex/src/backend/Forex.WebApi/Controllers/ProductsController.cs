namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Products.Commands;
using Forex.Application.Features.Products.Queries;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using VoltStream.WebApi.Controllers;

public class ProductsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProductCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{Id:long}")]
    public async Task<IActionResult> Delete(long Id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteProductCommand(Id)) });

    [HttpGet("{Id:long}")]
    public async Task<IActionResult> GetById(long Id)
        => Ok(new Response { Data = await Mediator.Send(new GetProductByIdQuery(Id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllProductsQuery()) });
}