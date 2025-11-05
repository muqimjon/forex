namespace Forex.Wpf.Pages.SemiProducts.Views;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.SemiProducts.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class SemiProductPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    public SemiProductPage()
    {
        InitializeComponent();
        DataContext = App.AppHost!.Services.GetRequiredService<SemiProductPageViewModel>();
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer scroll)
        {
            scroll.ScrollToVerticalOffset(scroll.VerticalOffset - e.Delta / 3.0);
            e.Handled = true;
        }
    }

    private void DataGrid_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is DataGrid grid)
        {
            grid.CommitEdit(DataGridEditingUnit.Row, true);
            grid.UnselectAll();
        }
    }
}
