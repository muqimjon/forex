namespace Forex.Wpf.Resources.UserControls;

using System.Windows;
using System.Windows.Controls;

public partial class FloatingInput : UserControl
{
    public FloatingInput()
    {
        InitializeComponent();
    }

    // Label property
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(FloatingInput),
            new PropertyMetadata(""));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // Text property (universal object type)
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(object),
            typeof(FloatingInput),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
