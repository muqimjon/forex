namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Auth.Commands;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]
public class AuthController : BaseController
{
    [HttpGet("ping")]
    public IActionResult Ping()
        => Ok(new Response { Data = "OK" });

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
        => Ok(new Response { Data = await Mediator.Send(command, Ct) });
}
