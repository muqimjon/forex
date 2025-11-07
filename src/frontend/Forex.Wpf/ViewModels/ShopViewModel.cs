namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class ShopViewModel : ViewModelBase
{
    public long Id { get; set; }
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private ObservableCollection<ShopAccountViewModel> shopAccounts = default!;
    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> productEntries = default!;
    [ObservableProperty] private ObservableCollection<ProductResidueViewModel> productResidues = default!;
    [ObservableProperty] private ObservableCollection<TransactionViewModel> transactions = default!;
}