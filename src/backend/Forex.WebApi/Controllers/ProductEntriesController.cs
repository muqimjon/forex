namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Products.ProductEntries.Commands;
using Forex.Application.Features.Products.ProductEntries.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class ProductEntriesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Entry(CreateProductEntryCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteProductEntryCommand(id)) });


    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(ProductEntryFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });
}