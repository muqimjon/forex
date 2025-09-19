namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Manufactories;
using Refit;

public interface IApiManufactory
{
    [Get("/manufactories")]
    Task<Response<List<ManufactoryResponse>>> GetAll();

    [Get("/manufactories/{id}")]
    Task<Response<ManufactoryResponse>> GetById(long id);

    [Post("/manufactories")]
    Task<Response<long>> Create([Body] ManufactoryResponse dto);

    [Put("/manufactories")]
    Task<Response<bool>> Update([Body] ManufactoryResponse dto);

    [Delete("/manufactories/{id}")]
    Task<Response<bool>> Delete(long id);
}
