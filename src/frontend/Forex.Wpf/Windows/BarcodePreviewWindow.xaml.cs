namespace Forex.Wpf.Windows;

using System.Windows;

public partial class BarcodePreviewWindow : Window
{
    public BarcodePreviewWindow(BarcodeLabelPreviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
