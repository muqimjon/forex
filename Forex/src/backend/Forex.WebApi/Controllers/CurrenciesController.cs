namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Currencies.Commands;
using Forex.Application.Features.Currencies.Queries;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using VoltStream.WebApi.Controllers;

public class CurrenciesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCurrencyCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateCurrencyCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{Id:long}")]
    public async Task<IActionResult> Delete(long Id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteCurrencyCommand(Id)) });

    [HttpGet("{Id:long}")]
    public async Task<IActionResult> GetById(long Id)
        => Ok(new Response { Data = await Mediator.Send(new GetCurrencyByIdQuery(Id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllCurrenciesQuery()) });
}
