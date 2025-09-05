namespace VoltStream.WebApi.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    private IMediator? mediator;
    protected IMediator Mediator
        => mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}
