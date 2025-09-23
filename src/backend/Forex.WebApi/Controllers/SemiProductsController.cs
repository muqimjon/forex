namespace Forex.WebApi.Controllers;

using Forex.Application.Features.SemiProductEntries.Commands;
using Forex.Application.Features.SemiProductEntries.DTOs;
using Forex.Application.Features.SemiProducts.Commands;
using Forex.Application.Features.SemiProducts.DTOs;
using Forex.Application.Features.SemiProducts.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class SemiProductsController
    : ReadOnlyController<SemiProductDto, GetAllSemiProductsQuery, GetSemiProductByIdQuery>
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

    [HttpPost("intake")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateIntake([FromForm] SemiProductIntakeRequest request)
    {
        var command = new CreateSemiProductIntakeCommand(
            request.SenderId,
            request.ManufactoryId,
            request.EntryDate,
            request.TransferFeePerContainer,
            request.Containers,
            request.Items.Select(i => new ItemDto(
                i.SemiProductId,
                i.Name,
                i.Code,
                i.Measure,
                i.Photo?.OpenReadStream(),
                i.Photo?.ContentType,
                Path.GetExtension(i.Photo?.FileName ?? string.Empty),
                i.Quantity,
                i.CostPrice,
                i.CostDelivery,
                i.TransferFee
            ))
        );

        return Ok(new Response { Data = await Mediator.Send(command) });
    }
}