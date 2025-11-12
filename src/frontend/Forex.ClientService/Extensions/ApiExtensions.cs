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
                using var doc = JsonDocument.Parse(apiEx.Content ?? "{}");
                var root = doc.RootElement;

                var statusCode = root.TryGetProperty("statusCode", out var statusProp)
                    ? statusProp.GetInt32()
                    : (int)apiEx.StatusCode;

                var message = root.TryGetProperty("message", out var messageProp)
                    ? messageProp.GetString() ?? apiEx.Message
                    : apiEx.Message;

                return new Response<T>
                {
                    StatusCode = statusCode,
                    Message = message
                };
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