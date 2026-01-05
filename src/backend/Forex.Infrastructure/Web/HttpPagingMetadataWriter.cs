namespace Forex.Infrastructure.Web;

using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

public class HttpPagingMetadataWriter(IHttpContextAccessor accessor) : IPagingMetadataWriter
{
    public void Write(PagedListMetadata metadata)
    {
        var headers = accessor.HttpContext?.Response?.Headers;
        if (headers is null) return;

        headers["X-Paging"] = JsonSerializer.Serialize(metadata);
    }
}
