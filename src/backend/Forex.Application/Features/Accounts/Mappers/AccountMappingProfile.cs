namespace Forex.Application.Features.Accounts.Mappers;

using AutoMapper;
using Forex.Application.Features.Accounts.Commands;
using Forex.Application.Features.Accounts.DTOs;
using Forex.Domain.Entities;

public class AccountMappingProfile : Profile
{
    public AccountMappingProfile()
    {
        // User Account
        CreateMap<CreateUserAccountCommand, UserAccount>();
        CreateMap<UpdateUserAccountCommand, UserAccount>();
        CreateMap<UserAccount, UserAccountDto>();
        CreateMap<UserAccount, AccountForUserDto>();

        // Shop Account
        CreateMap<CreateShopAccountCommand, ShopAccount>();
        CreateMap<UpdateShopAccountCommand, ShopAccount>();
        CreateMap<ShopAccount, ShopAccountDto>();
        CreateMap<ShopAccount, AccountForShopDto>();
    }
}
