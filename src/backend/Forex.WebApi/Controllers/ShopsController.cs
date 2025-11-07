namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Shops.Commands;
using Forex.Application.Features.Shops.DTOs;
using Forex.Application.Features.Shops.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class ShopsController
    : CrudController<ShopDto,
                     GetAllShopsQuery,
                     GetShopByIdQuery,
                     CreateShopCommand,
                     UpdateShopCommand,
                     DeleteShopCommand>
{
    [HttpPost("filter")]
    public async Task<IActionResult> GetFiltered(ShopFilterQuery query)
        => Ok(new Response { Data = await Mediator.Send(query) });
}