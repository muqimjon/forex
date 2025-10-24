namespace Forex.ClientService.Extensions;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Services.FileStorage.Minio;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Reflection;
using System.Text.Json;

public static class ApiExtensions
{
    private static IServiceProvider? _serviceProvider;

    // 🔹 DI orqali client service ichida bir marta chaqiriladi
    public static void Configure(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public static async Task<Response<T>> Handle<T>(
        this Task<Response<T>> task,
        Action<bool>? setLoading = null)
    {
        try
        {
            setLoading?.Invoke(true);

            var fileStorage = _serviceProvider?.GetService<MinioFileStorageService>();
            var requestObject = GetRefitRequestBody(task);

            if (fileStorage is not null && requestObject is not null)
                await UploadFilesRecursiveAsync(requestObject, fileStorage);

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

    // 🧩 Refit chaqiruvidan Body obyektni olish
    private static object? GetRefitRequestBody(Task task)
    {
        var field = task.GetType().GetField("m_action", BindingFlags.NonPublic | BindingFlags.Instance)
                   ?? task.GetType().GetField("m_action", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

        if (field?.GetValue(task) is Delegate del)
        {
            var target = del.Target;
            var bodyProp = target?.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => f.Name.Contains("Body"));
            return bodyProp?.GetValue(target);
        }

        return null;
    }

    // 🧩 Fayllarni yuklash logikasi (rekursiv)
    private static async Task UploadFilesRecursiveAsync(object? obj, MinioFileStorageService fileStorage)
    {
        if (obj == null)
            return;

        if (obj is IEnumerable<object> list)
        {
            foreach (var item in list)
                await UploadFilesRecursiveAsync(item, fileStorage);
            return;
        }

        // Faylli model
        var imageProp = obj.GetType().GetProperty("ImageBytes");
        var pathProp = obj.GetType().GetProperty("ImagePath");

        if (imageProp?.GetValue(obj) is byte[] bytes && bytes.Length > 0)
        {
            using var ms = new MemoryStream(bytes);
            var fileName = $"{Guid.NewGuid()}.png";
            var url = await fileStorage.UploadAsync(ms, fileName, "image/png");

            pathProp?.SetValue(obj, url);
            imageProp.SetValue(obj, null);
        }

        // Ichki propertylar uchun rekursiya
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                continue;

            var value = prop.GetValue(obj);
            if (value != null)
                await UploadFilesRecursiveAsync(value, fileStorage);
        }
    }
}
