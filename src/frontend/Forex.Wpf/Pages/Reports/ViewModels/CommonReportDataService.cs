namespace Forex.Wpf.Pages.Reports.ViewModels;

using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;


public partial class CommonReportDataService : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly IMapper _mapper;


    public ObservableCollection<UserViewModel> AvailableCustomers { get; } = [];
    public ObservableCollection<ProductViewModel> AvailableProducts { get; } = [];

    public CommonReportDataService(ForexClient client, IMapper mapper)
    {
        _client = client;
        _mapper = mapper;
        _ = LoadAsync(); // Bir marta yuklanadi, keyin cache
    }

    private async Task LoadAsync()
    {
        await Task.WhenAll(
            LoadCustomersAsync(),
            LoadProductsAsync()
        );
    }

    private async Task LoadCustomersAsync()
    {
        var request = new FilteringRequest
        {
            Filters = new()
            {
                ["role"] = ["mijoz"],
                ["accounts"] = ["include:currency"]
            }
        };

        var response = await _client.Users.Filter(request).Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
        {
            var customers = _mapper.Map<List<UserViewModel>>(response.Data);
            AvailableCustomers.Clear();
            foreach (var c in customers) AvailableCustomers.Add(c);
        }
    }

    private async Task LoadProductsAsync()
    {
        var response = await _client.Products.GetAllAsync().Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
        {
            var products = _mapper.Map<List<ProductViewModel>>(response.Data!);
            AvailableProducts.Clear();
            foreach (var p in products) AvailableProducts.Add(p);
        }
    }
}