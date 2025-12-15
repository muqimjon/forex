namespace Forex.Wpf.Resources.UserControls;

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class FloatingImageComboBox : UserControl
{
    public FloatingImageComboBox()
    {
        InitializeComponent();
    }

    // Label
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(FloatingImageComboBox),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // ItemsSource
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(FloatingImageComboBox),
            new PropertyMetadata(null));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    // SelectedItem
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(FloatingImageComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    // DropDownOpenedCommand
    public static readonly DependencyProperty DropDownOpenedCommandProperty =
        DependencyProperty.Register(nameof(DropDownOpenedCommand), typeof(ICommand), typeof(FloatingImageComboBox));

    public ICommand? DropDownOpenedCommand
    {
        get => (ICommand?)GetValue(DropDownOpenedCommandProperty);
        set => SetValue(DropDownOpenedCommandProperty, value);
    }

    public static readonly DependencyProperty LostFocusCommandProperty =
        DependencyProperty.Register(nameof(LostFocusCommand), typeof(ICommand), typeof(FloatingImageComboBox));

    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    private void ComboBox_DropDownOpened(object sender, EventArgs e)
    {
        if (DropDownOpenedCommand?.CanExecute(null) == true)
            DropDownOpenedCommand.Execute(null);
    }

    private void ComboBox_LostFocus(object sender, EventArgs e)
    {
        if (LostFocusCommand?.CanExecute(SelectedItem) == true)
            LostFocusCommand.Execute(SelectedItem);
    }
}
