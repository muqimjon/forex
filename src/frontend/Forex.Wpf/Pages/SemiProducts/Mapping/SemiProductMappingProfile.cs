namespace Forex.Wpf.Pages.SemiProducts.Mapping;

using AutoMapper;
using Forex.ClientService.Models.SemiProducts;
using Forex.Wpf.Pages.SemiProducts.ViewModels;

public class SemiProductMappingProfile : Profile
{
    public SemiProductMappingProfile()
    {
        CreateMap<SemiProductViewModel, SemiProductRequest>();
    }
}
