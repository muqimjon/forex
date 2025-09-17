namespace Forex.ClientService.Extensions;

using Forex.ClientService.Models.Commons;
using Refit;
using System.Text.Json;

public static class ApiExtensions
{
    public static async Task<Response<T>> Handle<T>(
    this Task<Response<T>> task,
    Action<bool>? setLoading = null)
    {
        try
        {
            setLoading?.Invoke(true);
            return await task;
        }
        catch (ApiException apiEx)
        {
            try
            {
                var problem = JsonSerializer.Deserialize<Response<T>>(apiEx.Content ?? "");
                if (problem is not null)
                    return problem;
            }
            catch { }

            return new Response<T>
            {
                StatusCode = (int)apiEx.StatusCode,
                Message = apiEx.ReasonPhrase ?? apiEx.Message
            };
        }
        catch (Exception ex)
        {
            return new Response<T>
            {
                StatusCode = 500,
                Message = ex.Message
            };
        }
        finally
        {
            setLoading?.Invoke(false);
        }
    }
}
