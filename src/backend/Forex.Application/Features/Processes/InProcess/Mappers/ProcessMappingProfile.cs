namespace Forex.Application.Features.Processes.InProcesses.Mappers;

using Forex.Application.Features.Processes.InProcess.DTOs;
using Forex.Application.Features.Sales.SaleItems.Mappers;
using Forex.Domain.Entities.Processes;

public class ProcessMappingProfile : MappingProfile
{
    public ProcessMappingProfile()
    {
        CreateMap<InProcess, InProcessDto>();
        CreateMap<EntryToProcess, EntryToProcessDto>();
    }
}
