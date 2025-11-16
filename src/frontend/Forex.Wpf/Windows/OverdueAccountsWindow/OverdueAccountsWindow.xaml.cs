namespace Forex.Wpf.Windows.OverdueAccountsWindow;

using Forex.Wpf.Windows.OverdueAccountsWindow.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public partial class OverdueAccountsWindow : Window
{
    public OverdueAccountsWindow()
    {
        InitializeComponent();
        DataContext = App.AppHost!.Services.GetRequiredService<OverdueAccountsViewModel>();
    }
    private void DueDate_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !char.IsDigit(e.Text, 0) && e.Text != ".";
    }

    // ...
    private void DueDate_GotFocus(object sender, RoutedEventArgs e)
    {
        var tb = sender as TextBox;
        tb?.SelectAll();
    }
    private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
    {
        if (e.EditingElement is TextBox tb)
        {
            tb.Focus();
            tb.SelectAll();
        }
    }

}
