namespace Forex.WebApi.Controllers;

using Forex.Application.Features.ShopCashes.Commands;
using Forex.Application.Features.ShopCashes.Queries;
using Forex.WebApi.Models.Commons;
using Microsoft.AspNetCore.Mvc;

public class ShopCashesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateShopCashCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateShopCashCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteShopCashCommand(id)) });

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
        => Ok(new Response { Data = await Mediator.Send(new GetShopCashByIdQuery(id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllShopCashesQuery()) });
}
