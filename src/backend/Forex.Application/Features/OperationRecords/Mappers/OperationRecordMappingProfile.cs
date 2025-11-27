namespace Forex.Application.Features.OperationRecords.Mappers;

using AutoMapper;
using Forex.Application.Features.OperationRecords.DTOs;
using Forex.Application.Features.Sales.DTOs;
using Forex.Application.Features.Transactions.DTOs;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Sales;

public class OperationRecordMappingProfile : Profile
{
    public OperationRecordMappingProfile()
    {
        CreateMap<OperationRecord, OperationRecordDto>()
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Description ?? ""));

        CreateMap<Sale, SaleForOperationDto>();

        CreateMap<Transaction, TransactionForOperationDto>();
    }
}
