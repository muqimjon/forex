namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class ProductResidueViewModel : ViewModelBase
{
    public long Id { get; set; }
    public long ShopId { get; set; }
    public long ProductTypeId { get; set; }

    [ObservableProperty] private int count;
    [ObservableProperty] private ProductTypeViewModel productType = default!;
    [ObservableProperty] private ShopViewModel shop = default!;
    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> productEntries = default!;
}