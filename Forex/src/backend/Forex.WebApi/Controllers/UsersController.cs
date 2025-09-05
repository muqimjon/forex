namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Users.Commands;
using Forex.Application.Features.Users.Queries;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using VoltStream.WebApi.Controllers;

public class UsersController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{Id:long}")]
    public async Task<IActionResult> Delete(long Id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteUserCommand(Id)) });

    [HttpGet("{Id:long}")]
    public async Task<IActionResult> GetById(long Id)
        => Ok(new Response { Data = await Mediator.Send(new GetUserByIdQuery(Id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllUsersQuery()) });
}
