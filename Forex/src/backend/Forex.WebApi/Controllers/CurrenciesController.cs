namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Currencies.Commands;
using Forex.Application.Features.Currencies.Queries;
using Forex.WebApi.Models.Commons;
using Microsoft.AspNetCore.Mvc;

public class CurrenciesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCurrencyCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateCurrencyCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteCurrencyCommand(id)) });

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
        => Ok(new Response { Data = await Mediator.Send(new GetCurrencyByIdQuery(id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllCurrenciesQuery()) });
}
