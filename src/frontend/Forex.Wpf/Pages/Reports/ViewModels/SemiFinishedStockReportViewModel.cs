namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Wordprocessing;
using global::Forex.ClientService;
using global::Forex.Wpf.Pages.Common;
using global::Forex.Wpf.Pages.Reports.ViewModels;
using global::Forex.Wpf.ViewModels;
using System.Collections.ObjectModel;

// SemiFinishedStockReportViewModel.cs
public partial class SemiFinishedStockReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    //[ObservableProperty] private ObservableCollection<SemiFinishedStockItemViewModel> items = [];

    public ObservableCollection<ProductViewModel> AvailableProducts => _commonData.AvailableProducts;
    [ObservableProperty] private ProductViewModel? selectedProduct;
    [ObservableProperty] private ProductViewModel? selectedCode;

    public SemiFinishedStockReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(SelectedProduct) or nameof(SelectedCode))
                _ = LoadAsync();
        };

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        //Items.Clear();

        //var response = await _client.Stocks.GetSemiFinished().Handle(l => IsLoading = l);
        //if (!response.IsSuccess || response.Data == null)
        //{ ErrorMessage = "Yarim tayyor mahsulotlar yuklanmadi"; return; }

        //foreach (var s in response.Data)
        //{
        //    var p = s.ProductType?.Product;
        //    if (SelectedProduct != null && p?.Id != SelectedProduct.Id) continue;
        //    if (SelectedCode != null && p?.Code != SelectedCode.Code) continue;

        //    Items.Add(new SemiFinishedStockItemViewModel
        //    {
        //        Name = p?.Name ?? "-",
        //        Type = s.ProductType?.Type ?? "-",
        //        BundleItemCount = s.ProductType?.BundleItemCount ?? 0,
        //        BundleCount = s.BundleCount,
        //        UnitMeasure = p?.UnitMeasure?.Name ?? "-",
        //        TotalCount = s.TotalCount,
        //        PurchasePrice = s.PurchasePrice,
        //        ExpensePerItem = s.ExpensePerItem,
        //        CostPrice = s.CostPrice,
        //        TotalAmount = s.TotalAmount
        //    });
        //}
    }
}