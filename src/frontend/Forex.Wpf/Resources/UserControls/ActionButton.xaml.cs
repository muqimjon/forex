namespace Forex.Wpf.Resources.UserControls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

public partial class ActionButton : UserControl
{
    public ActionButton()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (Application.Current.MainWindow is Window wnd)
                wnd.Deactivated += (_, _) => ClosePopup();
        };

        ActionPopup.Closed += (_, _) => ActionPopup.IsOpen = false;
    }

    #region Open/Close Popup Animations

    private void DotsButton_Click(object sender, RoutedEventArgs e)
    {
        AnimatePopup();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsEditingProperty, true); // bu modelga push qiladi
        ClosePopup();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(IsEditingProperty, false);
        ClosePopup();
    }

    private void ClosePopup()
    {
        ActionPopup.IsOpen = false;
        DotsButton.Visibility = Visibility.Visible;
    }

    private void AnimatePopup()
    {
        var scaleAnim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveAnim = new DoubleAnimation
        {
            From = 20,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        stackScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        stackScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        stackTranslate.BeginAnimation(TranslateTransform.XProperty, moveAnim);

        ActionPopup.IsOpen = true;
    }

    private void ClosePopup_Click(object sender, RoutedEventArgs e)
    {
        ClosePopup();
    }

    #endregion Open/Close Popup Animations

    #region DependencyProperties

    public static readonly DependencyProperty SaveCommandProperty =
    DependencyProperty.Register(nameof(SaveCommand), typeof(ICommand), typeof(ActionButton));

    public static readonly DependencyProperty EditCommandProperty =
        DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(ActionButton));

    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(ActionButton));

    public static readonly DependencyProperty IsEditingProperty =
    DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(ActionButton),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ActionButton));

    public ICommand EditCommand
    {
        get => (ICommand)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public ICommand DeleteCommand
    {
        get => (ICommand)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public ICommand SaveCommand
    {
        get => (ICommand)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public bool IsEditing
    {
        get => (bool)GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion
}
