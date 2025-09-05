namespace Forex.WebApi.Controllers;

using Forex.Application.Features.SemiProducts.Commands;
using Forex.Application.Features.SemiProducts.Queries;
using Forex.WebApi.Models;
using Forex.WebApi.Models.Commons;
using Microsoft.AspNetCore.Mvc;

public class SemiProductsController : BaseController
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] SemiProductRequest dto)
        => Ok(new Response
        {
            Data = await Mediator.Send(new SemiProductCommand(
            dto.ManufactoryId,
            dto.Name,
            dto.Code,
            dto.Measure,
            dto.Photo?.OpenReadStream(),
            Path.GetExtension(dto.Photo?.FileName ?? string.Empty),
            dto.Photo?.ContentType
        ))
        });

    [HttpPut("{id:long}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateSemiProduct(long id, [FromForm] SemiProductRequest dto)
        => Ok(new Response
        {
            Data = await Mediator.Send(new UpdateSemiProductCommand(
            id,
            dto.ManufactoryId,
            dto.Name,
            dto.Code,
            dto.Measure,
            dto.Photo?.OpenReadStream(),
            Path.GetExtension(dto.Photo?.FileName ?? string.Empty),
            dto.Photo?.ContentType
        ))
        });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteSemiProductCommand(id)) });

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
        => Ok(new Response { Data = await Mediator.Send(new GetSemiProductByIdQuery(id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllSemiProductsQuery()) });
}