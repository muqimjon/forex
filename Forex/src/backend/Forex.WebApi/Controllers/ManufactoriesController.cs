namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Manufactories.Commands;
using Forex.Application.Features.Manufactories.Queries;
using Forex.WebApi.Models.Commons;
using Microsoft.AspNetCore.Mvc;

public class ManufactoriesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateManufactoryCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateManufactoryCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteManufactoryCommand(id)) });

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
        => Ok(new Response { Data = await Mediator.Send(new GetManufactoryByIdQuery(id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllManufactoriesQuery()) });
}
