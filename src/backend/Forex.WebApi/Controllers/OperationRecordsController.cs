namespace Forex.WebApi.Controllers;

using Forex.Application.Features.OperationRecords.DTOs;
using Forex.Application.Features.OperationRecords.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class OperationRecordsController
    : ReadOnlyController<OperationRecordDto, GetAllOperationRecordsQuery, GetOperationRecordByIdQuery>
{

    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(OperationRecordFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });
}
