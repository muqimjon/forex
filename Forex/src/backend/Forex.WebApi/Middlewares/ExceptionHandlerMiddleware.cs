namespace Forex.WebApi.Middlewares;

using Forex.Application.Commons.Exceptions;
using Forex.WebApi.Models.Commons;

public class ExceptionHandlerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            context.Response.StatusCode = (int)ex.StatusCode;

            await context.Response.WriteAsJsonAsync(new Response
            {
                StatusCode = (int)ex.StatusCode,
                Message = ex.Message,
            });
        }
        catch
        {
            context.Response.StatusCode = 500;

            await context.Response.WriteAsJsonAsync(new
            {
                context.Response.StatusCode,
                Message = "An unexpected error occurred.",
            });
        }
    }
}
