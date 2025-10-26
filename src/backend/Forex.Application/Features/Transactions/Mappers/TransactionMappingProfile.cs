namespace Forex.Application.Features.Transactions.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Transactions.Commands;
using Forex.Application.Features.Transactions.DTOs;
using Forex.Domain.Entities;

public class TransactionMappingProfile : Profile
{
    public TransactionMappingProfile()
    {
        CreateMap<CreateTransactionCommand, Transaction>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToUtcSafe()));

        CreateMap<Transaction, TransactionDto>();
        CreateMap<Transaction, TransactionForShopDto>();
        CreateMap<Transaction, TransactionForUserDto>();
    }
}
