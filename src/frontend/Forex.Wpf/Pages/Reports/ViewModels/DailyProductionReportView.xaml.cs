namespace Forex.Wpf.Pages.Reports.ViewModels;

using Forex.Wpf.Common.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for DailyProductionReportView.xaml
/// </summary>
public partial class DailyProductionReportView : UserControl
{
    public DailyProductionReportView()
    {
        InitializeComponent();
        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();
    }

    private void RegisterFocusNavigation()
    {
        FocusNavigator.RegisterElements(
            [
                dateBegin.text,
                dateEnd.text,
                cbxCustomer,
                btnPreview,
                btnPrint,
                btnClear,
                btnExport,
            ]);

        FocusNavigator.FocusElement(cbxCustomer);
    }

    private void RegisterGlobalShortcuts()
    {
        btnPrint.RegisterShortcut(Key.P, ModifierKeys.Control);
    }
}
