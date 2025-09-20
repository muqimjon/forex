namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.SemiProducts;
using Refit;

public interface IApiSemiProduct
{
    [Get("/semi-products")]
    Task<Response<List<SemiProductResponse>>> GetAll();

    [Get("/semi-products/{id}")]
    Task<Response<SemiProductResponse>> GetById(long id);

    [Delete("/semi-products/{id}")]
    Task<Response<bool>> Delete(long id);

    [Multipart]
    [Post("/semi-products")]
    Task<Response<long>> Create([AliasAs("ManufactoryId")] int manufactoryId,
                                [AliasAs("Name")] string name,
                                [AliasAs("Code")] int code,
                                [AliasAs("Measure")] string measure,
                                [AliasAs("Photo")] StreamPart photo);

    [Multipart]
    [Put("/semi-products/{id}")]
    Task<Response<long>> Update(long id,
                                [AliasAs("ManufactoryId")] int manufactoryId,
                                [AliasAs("Name")] string name,
                                [AliasAs("Code")] int code,
                                [AliasAs("Measure")] string measure,
                                [AliasAs("Photo")] StreamPart photo);

    [Post("/semi-products/intake")]
    Task<Response<bool>> CreateIntake([Body] MultipartFormDataContent content);
}
