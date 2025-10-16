namespace Forex.Application.Features.Users.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Users.Commands;
using Forex.Application.Features.Users.DTOs;
using Forex.Domain.Entities;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserCommand, User>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()))
            .ForMember(dest => dest.NormalizedEmail,
                opt => opt.MapFrom(src => src.Email!.ToNormalized()));

        CreateMap<UpdateUserCommand, User>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()))
            .ForMember(dest => dest.NormalizedEmail,
                opt => opt.MapFrom(src => src.Email!.ToNormalized()));

        CreateMap<User, UserForSaleDto>();
        CreateMap<User, UserForTransactionDto>();
        CreateMap<User, UserForProductEntryDto>();
        CreateMap<User, UserForAccountDto>();
        CreateMap<User, UserForInvoiceDto>();

        CreateMap<UserCommand, User>();
    }
}
