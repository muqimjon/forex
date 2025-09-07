namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Auth.Commands;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class AuthController : BaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
        => Ok(new Response { Data = await Mediator.Send(command) });
}