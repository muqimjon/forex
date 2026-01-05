namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Files.Queries.GetPresignedUrl;
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

    [HttpPost("image/upload-url")]
    public async Task<IActionResult> GenerateImageUploadUrl([FromBody] GenerateUploadUrlRequest request)
    {
        var result = await Mediator.Send(new GetPresignedUrlQuery(request.FileName, "products"));
        return Ok(new Response { Data = result });
    }
}

public sealed record GenerateUploadUrlRequest
{
    public required string FileName { get; init; }
}
