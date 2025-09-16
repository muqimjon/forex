namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.SemiProducts;
using Refit;

public interface IApiSemiProduct
{
    [Get("/SemiProducts")]
    Task<Response<List<SemiProductDto>>> GetAll();

    [Get("/SemiProducts/{id}")]
    Task<Response<SemiProductDto>> GetById(long id);

    [Delete("/SemiProducts/{id}")]
    Task<Response<bool>> Delete(long id);

    [Multipart]
    [Post("/SemiProducts")]
    Task<Response<long>> Create([AliasAs("ManufactoryId")] int manufactoryId,
                                [AliasAs("Name")] string name,
                                [AliasAs("Code")] int code,
                                [AliasAs("Measure")] string measure,
                                [AliasAs("Photo")] StreamPart photo);

    [Multipart]
    [Put("/SemiProducts/{id}")]
    Task<Response<long>> Update(long id,
                                [AliasAs("ManufactoryId")] int manufactoryId,
                                [AliasAs("Name")] string name,
                                [AliasAs("Code")] int code,
                                [AliasAs("Measure")] string measure,
                                [AliasAs("Photo")] StreamPart photo);

    [Post("/SemiProducts/intake")]
    Task<Response<bool>> CreateIntake([Body] MultipartFormDataContent content);
}
