namespace Forex.Application.Common.Interfaces;

using Forex.Application.Common.Models;

public interface IPagingMetadataWriter
{
    void Write(PagedListMetadata metadata);
}
