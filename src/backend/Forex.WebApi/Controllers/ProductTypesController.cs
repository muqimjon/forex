namespace Forex.WebApi.Controllers;

using Forex.Application.Features.Products.ProductTypes.Queries;
using Forex.WebApi.Controllers.Common;
using Forex.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ProductTypesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(new Response { Data = await Mediator.Send(new GetAllProductTypesQuery()) });
}
