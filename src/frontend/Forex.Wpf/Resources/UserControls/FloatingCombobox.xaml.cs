namespace Forex.Wpf.Resources.UserControls;

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class FloatingComboBox : UserControl
{
    public FloatingComboBox()
    {
        InitializeComponent();
    }

    // Label
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(FloatingComboBox),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // Text
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(FloatingComboBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // ItemsSource
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(FloatingComboBox),
            new PropertyMetadata(null));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    // DisplayMemberPath
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(FloatingComboBox),
            new PropertyMetadata(string.Empty));

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    // SelectedItem
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(FloatingComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    // IsEditable
    public static readonly DependencyProperty IsEditableProperty =
        DependencyProperty.Register(nameof(IsEditable), typeof(object), typeof(FloatingComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? IsEditable
    {
        get => GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    // IsTextSearchEnabled
    public static readonly DependencyProperty IsTextSearchEnabledProperty =
        DependencyProperty.Register(nameof(IsTextSearchEnabled), typeof(object), typeof(FloatingComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? IsTextSearchEnabled
    {
        get => GetValue(IsTextSearchEnabledProperty);
        set => SetValue(IsTextSearchEnabledProperty, value);
    }

    // SelectedValue/SelectedValuePath passthrough — kerak bo'lsa yoqing
    public static readonly DependencyProperty SelectedValuePathProperty =
        DependencyProperty.Register(nameof(SelectedValuePath), typeof(string), typeof(FloatingComboBox),
            new PropertyMetadata(string.Empty));

    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(nameof(SelectedValue), typeof(object), typeof(FloatingComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public static readonly DependencyProperty DropDownOpenedCommandProperty =
        DependencyProperty.Register(
            nameof(DropDownOpenedCommand),
            typeof(ICommand),
            typeof(FloatingComboBox),
            new PropertyMetadata(null));

    public ICommand? DropDownOpenedCommand
    {
        get => (ICommand?)GetValue(DropDownOpenedCommandProperty);
        set => SetValue(DropDownOpenedCommandProperty, value);
    }

    // LostFocus Command uchun DependencyProperty
    public static readonly DependencyProperty LostFocusCommandProperty =
        DependencyProperty.Register(
            nameof(LostFocusCommand),
            typeof(ICommand),
            typeof(FloatingComboBox),
            new PropertyMetadata(null));

    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    // Ichki ComboBox'ning DropDownOpened voqeasida command'ni chaqiramiz
    private void ComboBox_DropDownOpened(object sender, EventArgs e)
    {
        if (DropDownOpenedCommand?.CanExecute(null) == true)
            DropDownOpenedCommand.Execute(null);
    }

    // Ichki ComboBox'ning LostFocus voqeasida command'ni chaqiramiz
    private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (LostFocusCommand?.CanExecute(Text) == true)
            LostFocusCommand.Execute(Text);
    }

    // Ichki ComboBox'ga to'g'ridan-to'g'ri kirish kerak bo'lsa
    public ComboBox ComboBox => comboBox;
}