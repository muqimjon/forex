namespace Forex.WebApi.Controllers;

using Forex.Application.Features.SemiProductEntries.Commands;
using Forex.Application.Features.SemiProducts.Commands;
using Forex.Application.Features.SemiProducts.DTOs;
using Forex.Application.Features.SemiProducts.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class SemiProductsController
    : ReadOnlyController<SemiProductDto, GetAllSemiProductsQuery, GetSemiProductByIdQuery>
{
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

    [HttpPost("intake")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateIntake([FromForm] CreateSemiProductIntakeCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });
}