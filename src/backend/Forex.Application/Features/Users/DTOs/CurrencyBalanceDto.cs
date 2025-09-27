namespace Forex.Application.Features.Users.DTOs;

public record CurrencyBalanceDto(long CurrencyId, decimal Balance, decimal Discount, bool IsDefault);
