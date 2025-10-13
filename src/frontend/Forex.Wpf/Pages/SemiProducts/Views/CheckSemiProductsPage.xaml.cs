namespace Forex.Wpf.Pages.SemiProducts.Views;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.SemiProducts.ViewModels;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for CheckSemiProductsPage.xaml
/// </summary>
public partial class CheckSemiProductsPage : Page
{
    public CheckSemiProductsPage(SemiProductPageViewModel FlatRows)
    {
        DataContext = FlatRows;
        InitializeComponent();

        FocusNavigator.AttachEnterNavigation([
            submitButton
            ]);
    }

    private void StackPanel_ManipulationInertiaStarting(object sender, System.Windows.Input.ManipulationInertiaStartingEventArgs e)
    {

    }
}
