namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Users.Commands;
using Forex.Application.Features.Users.Queries;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]
public class UsersController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteUserCommand(id)) });

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
        => Ok(new Response { Data = await Mediator.Send(new GetUserByIdQuery(id)) });

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(new Response { Data = await Mediator.Send(new GetAllUsersQuery()) });

    [HttpGet("filter")]
    public async Task<IActionResult> GetFiltered()
        => Ok(new Response { Data = await Mediator.Send(new GetAllUsersQuery()) });
}
