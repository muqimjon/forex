namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Currencies.Commands;
using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.Currencies.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class CurrenciesController
    : CrudController<CurrencyDto,
                     GetAllCurrenciesQuery,
                     GetCurrencyByIdQuery,
                     CreateCurrencyCommand,
                     UpdateCurrencyCommand,
                     DeleteCurrencyCommand>
{
    [HttpPut("all")]
    public async Task<IActionResult> Entry(UpdateAllCurrenciesCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });
}
