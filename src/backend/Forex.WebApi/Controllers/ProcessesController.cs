namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Processes.EntryToProcesses.Commands;
using Forex.Application.Features.Processes.InProcess.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class ProcessesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Entry(List<EntryToProcessCommand> commands)
        => Ok(new Response { Data = await Mediator.Send(new CreateEntryToProcessCommand(commands)) });

    [HttpPut]
    public async Task<IActionResult> Edit(EditEntryToProcessCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete]
    public async Task<IActionResult> Delete(DeleteEntryToProcessCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(InProcessFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });
}