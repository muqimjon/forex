namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class SaleViewModel : ViewModelBase
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    [ObservableProperty] private DateTime date;
    [ObservableProperty] private decimal costPrice;
    [ObservableProperty] private decimal benifitPrice;
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private decimal totalAmount;
    [ObservableProperty] private string? note;

    [ObservableProperty] private UserResponse customer = new();
    [ObservableProperty] private ObservableCollection<SaleItemResponse> saleItems = [];
}