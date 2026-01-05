namespace Forex.ClientService.Models.Responses;

public record PresignedUrlResponse(string Url, string Key, DateTime ExpiresAt);
