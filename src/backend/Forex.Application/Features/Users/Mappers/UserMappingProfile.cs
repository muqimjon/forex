namespace Forex.Application.Features.Users.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Users.Commands;
using Forex.Application.Features.Users.DTOs;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Users;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Account, AccountDto>();
        CreateMap<CreateUserCommand, User>()
            .ForMember(dest => dest.SearchName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()))
            .ForMember(dest => dest.SearchEmail,
                opt => opt.MapFrom(src => src.Email!.ToNormalized()));

        CreateMap<UpdateUserCommand, User>()
            .ForMember(dest => dest.SearchName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()))
            .ForMember(dest => dest.SearchEmail,
                opt => opt.MapFrom(src => src.Email!.ToNormalized()));
    }
}
