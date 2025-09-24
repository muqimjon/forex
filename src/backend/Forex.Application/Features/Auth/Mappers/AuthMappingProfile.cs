namespace Forex.Application.Features.Auth.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Auth.Commands;
using Forex.Domain.Entities.Users;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<RegisterCommand, User>()
            .ForMember(dest => dest.SearchName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()))
            .ForMember(dest => dest.SearchEmail,
                opt => opt.MapFrom(src => src.Email!.ToNormalized()));
    }
}
