namespace Forex.ClientService.Models.Requests;

public sealed record GenerateUploadUrlRequest
{
    public required string FileName { get; init; }
}
