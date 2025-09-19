namespace Forex.Infrastructure.Web;

using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

public class HttpPagingMetadataWriter : IPagingMetadataWriter
{
    private readonly IHttpContextAccessor accessor;

    public HttpPagingMetadataWriter(IHttpContextAccessor accessor)
    {
        this.accessor = accessor;
        PagingExtensions.ConfigureWriter(this);
    }

    public void Write(PagedListMetadata metadata)
    {
        var headers = accessor.HttpContext?.Response?.Headers;
        if (headers is null) return;

        headers["X-Paging"] = JsonSerializer.Serialize(metadata);
    }
}


