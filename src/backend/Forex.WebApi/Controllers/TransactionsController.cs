namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Transactions.Commands;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class TransactionsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateTransactionCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteTransactionCommand(id)) });
}