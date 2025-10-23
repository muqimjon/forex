namespace Forex.WebApi.Controllers;

using Forex.Application.Features.SemiProducts.SemiProductEntries.Commands;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class SemiProductEntriesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateSemiProductIntakeCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{invoiceId:long}")]
    public async Task<IActionResult> Delete(long invoiceId)
        => Ok(new Response { Data = await Mediator.Send(new DeleteSemiProductIntakeCommand(invoiceId)) });
}