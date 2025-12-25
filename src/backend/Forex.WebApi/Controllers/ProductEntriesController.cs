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

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProductEntryCommand query)
        => Ok(new Response { Data = await Mediator.Send(query) });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteProductEntryCommand(id)) });


    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(ProductEntryFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });

    [HttpPost("image-entry")]
    public async Task<IActionResult> EntryWithImage(CreateProductEntryWithImageCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpGet("presigned-url")]
    public async Task<IActionResult> GetPresignedUrl([FromQuery] string extension)
        => Ok(new Response { Data = await Mediator.Send(new GetPresignedUrlQuery("", extension)) });
}