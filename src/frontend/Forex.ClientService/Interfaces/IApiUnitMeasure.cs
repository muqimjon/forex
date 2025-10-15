namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiUnitMeasure
{
    [Get("/unit-measures")]
    Task<Response<List<UnitMeasureResponse>>> GetAll();

    [Post("/unit-measures/filter")]
    Task<Response<List<UnitMeasureResponse>>> Filter(FilteringRequest request);

    [Get("/unit-measures/{id}")]
    Task<Response<UnitMeasureResponse>> GetById(long id);

    [Post("/unit-measures")]
    Task<Response<long>> Create([Body] UnitMeasureRequest request);

    [Put("/unit-measures")]
    Task<Response<bool>> Update([Body] UnitMeasureRequest request);

    [Delete("/unit-measures/{id}")]
    Task<Response<bool>> Delete(long id);
}