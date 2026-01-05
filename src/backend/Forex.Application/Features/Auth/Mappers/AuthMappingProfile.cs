namespace Forex.Application.Features.Auth.Mappers;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Features.Auth.Commands;
using Forex.Domain.Entities;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<RegisterCommand, User>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()))
            .ForMember(dest => dest.NormalizedEmail,
                opt => opt.MapFrom(src => src.Email!.ToNormalized()));
    }
}
