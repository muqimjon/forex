namespace Forex.Application.Features.Users.Mappers;

using AutoMapper;
using Forex.Application.Features.Users.Commands;
using Forex.Domain.Entities;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<CreateUserCommand, User>();
    }
}
