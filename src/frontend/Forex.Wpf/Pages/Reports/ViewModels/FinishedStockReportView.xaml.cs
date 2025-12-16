namespace Forex.Wpf.Pages.Reports.ViewModels;

using Forex.Wpf.Common.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for FinishedStockReportView.xaml
/// </summary>
public partial class FinishedStockReportView : UserControl
{
    public FinishedStockReportView()
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
            cbxProductCode,
            cbxProductName,
            btnPreview,
            btnPrint,
            btnClear,
            btnExport,
        ]);
    }

    private void RegisterGlobalShortcuts()
    {
        btnPrint.RegisterShortcut(Key.P, ModifierKeys.Control);
    }
}
