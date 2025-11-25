namespace Forex.Application.Features.OperationRecords.Mappers;

using AutoMapper;
using Forex.Application.Features.OperationRecords.DTOs;
using Forex.Domain.Entities;

public class OperationRecordMappingProfile : Profile
{
    public OperationRecordMappingProfile()
    {
        CreateMap<OperationRecord, OperationRecordDto>();
    }
}
