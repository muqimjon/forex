namespace Forex.Wpf.Pages.Products.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Products.Views;
using Forex.Wpf.ViewModels;
using Forex.Wpf.Windows;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

public partial class ProductPageViewModel : ViewModelBase
{
    private readonly ForexClient Client = App.AppHost!.Services.GetRequiredService<ForexClient>();
    private readonly IMapper Mapper = App.AppHost!.Services.GetRequiredService<IMapper>();
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;

    public ProductPageViewModel()
    {
        _ = LoadDataAsync();
    }

    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> productEntries = [];
    [ObservableProperty] private DateTime beginDate = DateTime.Now;
    [ObservableProperty] private DateTime endDate = DateTime.Now.AddDays(1);

    #region Load Data

    private async Task LoadDataAsync()
    {
        await LoadTransactionAsunc();
    }

    private async Task LoadTransactionAsunc()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["producttype"] = ["include:product"],
                ["date"] = [$"{BeginDate:yyyy-MM-dd}"]
            }
        };

        var response = await Client.ProductEntries.Filter(request).Handle();
        if (response.IsSuccess)
            ProductEntries = Mapper.Map<ObservableCollection<ProductEntryViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "Product kirimi ma'lumotlarini yuklashda xatolik!";
    }

    #endregion Load Data

    #region Commands

    [RelayCommand]
    private void RedirectToAddPage()
    {
        Main.NavigateTo(new ProductEntryPage());
    }

    #endregion Commands
}