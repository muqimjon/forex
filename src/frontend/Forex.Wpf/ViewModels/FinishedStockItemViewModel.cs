namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;


public partial class FinishedStockItemViewModel : ObservableObject
{
    [ObservableProperty] private string code = "-";
    [ObservableProperty] private string name = "-";
    [ObservableProperty] private string type = "-";
    [ObservableProperty] private int bundleItemCount;
    [ObservableProperty] private string? bundleCount;
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private decimal unitPrice;
    [ObservableProperty] private decimal totalAmount;

    // Qo‘shimcha: agar kerak bo‘lsa
    partial void OnTotalCountChanged(int value)
    {
        if (BundleItemCount > 0)
        {
            int fullBags = TotalCount / BundleItemCount;
            int remainder = TotalCount % BundleItemCount;

            if (remainder == 0)
                BundleCount = $"{fullBags} ta qop";
            else
                BundleCount = $"{fullBags} ta qop {remainder} dona";
        }
        else
        {
            BundleCount = "-";
        }
    }


}