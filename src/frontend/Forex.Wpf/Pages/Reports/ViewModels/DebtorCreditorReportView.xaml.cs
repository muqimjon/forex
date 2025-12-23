namespace Forex.Wpf.Pages.Reports.ViewModels;

using System.Windows.Controls;


/// <summary>
/// Interaction logic for DebtorCreditorReportView.xaml
/// </summary>
public partial class DebtorCreditorReportView : UserControl
{
    public DebtorCreditorReportView()
    {
        InitializeComponent();
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
