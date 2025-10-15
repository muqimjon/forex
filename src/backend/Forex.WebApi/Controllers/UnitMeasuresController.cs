namespace Forex.WebApi.Controllers;

using Forex.Application.Features.UnitMeasures.Commands;
using Forex.Application.Features.UnitMeasures.DTOs;
using Forex.Application.Features.UnitMeasures.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class UnitMeasuresController
    : CrudController<UnitMeasureDto,
                     GetAllUnitMeasuresQuery,
                     GetUnitMeasureByIdQuery,
                     CreateUnitMeasureCommand,
                     UpdateUnitMeasureCommand,
                     DeleteUnitMeasureCommand>
{
    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(UnitMeasureFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });
}