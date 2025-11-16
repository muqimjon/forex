namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Accounts.Commands;
using Forex.Application.Features.Accounts.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class UserAccountController : BaseController
{
    [HttpPut]
    public async Task<IActionResult> Update(UpdateForDebtUserAccountCommand request)
        => Ok(new Response{ Data = await Mediator.Send(request)});

    [HttpGet]
    public async Task<IActionResult> GetAll() 
        => Ok(new Response { Data = await Mediator.Send(new GetAllAccountsQuery()) });
}