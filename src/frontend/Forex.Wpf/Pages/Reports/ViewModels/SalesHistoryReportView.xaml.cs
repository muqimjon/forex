namespace Forex.Wpf.Pages.Reports.ViewModels;

using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for SalesHistoryReportView.xaml
/// </summary>
public partial class SalesHistoryReportView : UserControl
{
    public SalesHistoryReportView()
    {
        InitializeComponent();
    }

    private async void EndDate_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is SalesHistoryReportViewModel vm)
        {
            await vm.LoadAsync();
        }
    }
}
