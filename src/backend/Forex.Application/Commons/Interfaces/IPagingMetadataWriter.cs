namespace Forex.Application.Commons.Interfaces;

using Forex.Application.Commons.Models;

public interface IPagingMetadataWriter
{
    void Write(PagedListMetadata metadata);
}
