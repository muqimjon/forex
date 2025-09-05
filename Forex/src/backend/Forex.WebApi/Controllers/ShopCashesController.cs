namespace Forex.WebApi.Controllers;

using Forex.Application.Features.ShopCashes.Commands;
using Forex.Application.Features.ShopCashes.Queries;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using VoltStream.WebApi.Controllers;

public class ShopCashesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateShopCashCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateShopCashCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{Id:long}")]
    public async Task<IActionResult> Delete(long Id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteShopCashCommand(Id)) });

    [HttpGet("{Id:long}")]
    public async Task<IActionResult> GetById(long Id)
        => Ok(new Response { Data = await Mediator.Send(new GetShopCashByIdQuery(Id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllShopCashesQuery()) });
}
