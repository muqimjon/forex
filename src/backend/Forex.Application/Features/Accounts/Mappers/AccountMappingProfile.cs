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
        CreateMap<CreateUserAccountCommand, UserAccount>()
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(src => src.OpeningBalance));

        CreateMap<UpdateUserAccountCommand, UserAccount>()
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(src => src.OpeningBalance));

        CreateMap<UserAccount, UserAccountDto>();
        CreateMap<UserAccount, AccountForUserDto>();

        // Shop Account
        CreateMap<CreateShopAccountCommand, ShopAccount>()
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(src => src.OpeningBalance));

        CreateMap<UpdateShopAccountCommand, ShopAccount>()
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(src => src.OpeningBalance));

        CreateMap<ShopAccount, ShopAccountDto>();
        CreateMap<ShopAccount, AccountForShopDto>();
    }
}
