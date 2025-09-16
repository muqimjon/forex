namespace Forex.Wpf.Resources.UserControls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class FloatingComboBox : UserControl
{
    public FloatingComboBox()
    {
        InitializeComponent();
        comboBox.PreviewKeyDown += ComboBox_PreviewKeyDown;
    }

    private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            comboBox.IsDropDownOpen = !comboBox.IsDropDownOpen;
            e.Handled = true;
        }
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(FloatingComboBox), new PropertyMetadata(""));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(FloatingComboBox), new PropertyMetadata(""));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
