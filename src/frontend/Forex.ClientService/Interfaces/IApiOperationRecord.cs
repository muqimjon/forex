namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiOperationRecord
{
    [Post("/operation-records")]
    Task<Response<OperationRecordTurnoverDto>> GetTurnover([Body] TurnoverRequest request);
}