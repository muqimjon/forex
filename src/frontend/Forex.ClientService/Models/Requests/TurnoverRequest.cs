namespace Forex.ClientService.Models.Requests;

public record TurnoverRequest(long UserId, DateTime Begin, DateTime End);
