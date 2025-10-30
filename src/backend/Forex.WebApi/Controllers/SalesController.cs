namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Sales.Commands;
using Forex.Application.Features.Sales.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class SalesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Entry(CreateSaleCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteSaleCommand(id)) });

    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(SaleFilterQuery query)
       => Ok(new Response { Data = await Mediator.Send(query) });
}
