namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Shops.Commands;
using Forex.Application.Features.Shops.Queries;
using Forex.WebApi.Models.Commons;
using Microsoft.AspNetCore.Mvc;

public class ShopsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateShopCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateShopCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteShopCommand(id)) });

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
        => Ok(new Response { Data = await Mediator.Send(new GetShopByIdQuery(id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllShopsQuery()) });
}
