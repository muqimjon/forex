namespace Forex.Wpf.Pages.SemiProducts.UserControls;

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class EditableGridControl : UserControl
{
    public EditableGridControl()
    {
        InitializeComponent();
    }

    // Title
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(EditableGridControl), new PropertyMetadata(""));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // ItemsSource
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(EditableGridControl), new PropertyMetadata(null));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    // AddCommand
    public static readonly DependencyProperty AddCommandProperty =
        DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(EditableGridControl), new PropertyMetadata(null));

    public ICommand AddCommand
    {
        get => (ICommand)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }
}
