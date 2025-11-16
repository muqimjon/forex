namespace Forex.Application.Features.Accounts.Mappers;

using AutoMapper;
using Forex.Application.Features.Accounts.Commands;
using Forex.Application.Features.Accounts.DTOs;
using Forex.Domain.Entities;

public class AccountMappingProfile : Profile
{
    public AccountMappingProfile()
    {
        // Customer Account
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
        CreateMap<UserAccount, UserAccountDto>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.OpeningBalance, opt => opt.MapFrom(src => src.OpeningBalance))
             .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
             .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance))
             .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.CurrencyId))
             .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
             .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
             .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
             .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate));
    }
}
