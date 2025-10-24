namespace Forex.WebApi.Controllers;

using Forex.Application.Features.SemiProducts.SemiProducts.Commands;
using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;
using Forex.Application.Features.SemiProducts.SemiProducts.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

public class SemiProductsController
    : ReadOnlyController<SemiProductDto, GetAllSemiProductsQuery, GetSemiProductByIdQuery>
{

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
        => Ok(new Response { Data = await Mediator.Send(new DeleteSemiProductCommand(id)) });
}
