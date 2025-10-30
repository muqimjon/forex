namespace Forex.Wpf.Pages.Settings;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Settings.ViewModels;
using Forex.Wpf.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly SettingsPageViewModel vm;

    public SettingsPage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<SettingsPageViewModel>();
        DataContext = vm;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }


    private CurrencyViewModel? _draggedItem;

    private void DragHandle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var element = sender as FrameworkElement;
        _draggedItem = (element?.DataContext as CurrencyViewModel)!;
    }

    private void DragHandle_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
        {
            DragDrop.DoDragDrop((DependencyObject)sender, _draggedItem, DragDropEffects.Move);
        }
    }

    private void DragHandle_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _draggedItem = null!;
    }

    private void ItemsControl_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(CurrencyViewModel)) is CurrencyViewModel draggedItem && ((FrameworkElement)e.OriginalSource).DataContext is CurrencyViewModel targetItem && draggedItem != targetItem)
        {
            var list = vm.Currencies;
            int oldIndex = list.IndexOf(draggedItem);
            int newIndex = list.IndexOf(targetItem);

            list.Move(oldIndex, newIndex);

            for (int i = 0; i < list.Count; i++)
            {
                list[i].Position = i + 1;
            }
        }
    }
}
