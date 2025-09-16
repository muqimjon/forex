namespace Forex.Wpf.Resources.UserControls;

using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

public partial class FloatingFileInput : UserControl
{
    public FloatingFileInput()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(FloatingFileInput), new PropertyMetadata(""));

    public static readonly DependencyProperty FileNameProperty =
    DependencyProperty.Register(
        nameof(FileName),
        typeof(string),
        typeof(FloatingFileInput),
        new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnFileNameChanged));

    private static void OnFileNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FloatingFileInput)d;

        if (e.NewValue is null)
            control.fileNameBox.Text = string.Empty;
    }


    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string FileName
    {
        get => (string)GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Rasm tanlang",
            Filter = "Rasm fayllari (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Barcha fayllar (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            FileName = dialog.FileName;
        }
    }
}
