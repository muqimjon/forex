namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Manufactory;
using Refit;

public interface IApiManufactory
{
    [Get("/manufactories")]
    Task<Response<List<ManufactoryDto>>> GetAll();

    [Get("/manufactories/{id}")]
    Task<Response<ManufactoryDto>> GetById(long id);

    [Post("/manufactories")]
    Task<Response<long>> Create([Body] ManufactoryDto dto);

    [Put("/manufactories")]
    Task<Response<bool>> Update([Body] ManufactoryDto dto);

    [Delete("/manufactories/{id}")]
    Task<Response<bool>> Delete(long id);
}
