namespace Forex.Wpf.Pages.Reports.ViewModels;

using Forex.Wpf.Common.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


/// <summary>
/// Interaction logic for DebtorCreditorReportView.xaml
/// </summary>
public partial class DebtorCreditorReportView : UserControl
{
    public DebtorCreditorReportView()
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

    private void TextBlock_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is TextBlock textBlock)
        {
            // DataGridRow klassi System.Windows.Controls ichida
            var row = DataGridRow.GetRowContainingElement(textBlock);
            if (row != null)
            {
                textBlock.Text = (row.GetIndex() + 1).ToString();
            }
        }
    }
}
