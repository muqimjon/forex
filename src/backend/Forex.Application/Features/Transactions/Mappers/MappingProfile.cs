namespace Forex.Application.Features.Transactions.Mappers;

using AutoMapper;
using Forex.Application.Features.Transactions.DTOs;
using System.Transactions;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Transaction, TransactionDto>();
        CreateMap<Transaction, TransactionForShopDto>();
        CreateMap<Transaction, TransactionForUserDto>();
    }
}
