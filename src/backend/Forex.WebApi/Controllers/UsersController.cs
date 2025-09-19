namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Users.Commands;
using Forex.Application.Features.Users.DTOs;
using Forex.Application.Features.Users.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class UsersController
    : CrudController<UserDto,
                     GetAllUsersQuery,
                     GetUserByIdQuery,
                     CreateUserCommand,
                     UpdateUserCommand,
                     DeleteUserCommand>
{
    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(UserFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });
}
