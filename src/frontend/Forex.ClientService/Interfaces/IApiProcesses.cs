namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Refit;

public interface IApiProcesses
{
    [Post("/processes")]
    Task<Response<long>> CreateAsync(List<EntryToProcessRequest> request);

    [Put("/processes")]
    Task<Response<bool>> EditAsync(EntryToProcessRequest request);

    [Delete("/processes/{id}")]
    Task<Response<bool>> DeleteAsync(long id);
}
