namespace Forex.WebApi.Middlewares;

using Forex.Application.Common.Exceptions;
using Forex.WebApi.Models;
using Microsoft.EntityFrameworkCore;

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
        catch (DbUpdateConcurrencyException)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new Response
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Ma'lumot boshqa amaliyot tomonidan o'zgartirildi. Iltimos, qayta urinib ko'ring.",
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("========== XATOLIK TAFSILOTI ==========");
            Console.WriteLine($"Xato turi: {ex.GetType().Name}");
            Console.WriteLine($"Xabar: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            Console.WriteLine("=======================================");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new Response
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Ichki xatolik yuz berdi.",
            });
        }
    }
}
