namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Cashes.Commands;
using Forex.Application.Features.Cashes.Queries;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using VoltStream.WebApi.Controllers;

public class CashesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCashCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateCashCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{Id:long}")]
    public async Task<IActionResult> Delete(long Id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteCashCommand(Id)) });

    [HttpGet("{Id:long}")]
    public async Task<IActionResult> GetById(long Id)
        => Ok(new Response { Data = await Mediator.Send(new GetCashByIdQuery(Id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllCashesQuery()) });
}
