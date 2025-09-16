namespace Forex.Wpf.Resources.UserControls;

using Forex.Wpf.Common.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

public partial class ActionButton : UserControl
{
    public ActionButton()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (Application.Current.MainWindow is Window wnd)
            {
                wnd.Deactivated += OnWindowDeactivated;
            }
        };

        Unloaded += (_, _) =>
        {
            if (Application.Current.MainWindow is Window wnd)
            {
                wnd.Deactivated -= OnWindowDeactivated;
            }
        };

        ActionPopup.Closed += (_, _) => DotsButton.Visibility = Visibility.Visible;
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        if (ActionPopup.IsOpen)
        {
            ActionPopup.IsOpen = false;
            DotsButton.Visibility = Visibility.Visible;
        }
    }

    #region DependencyProperties

    public static readonly DependencyProperty EditCommandProperty =
        DependencyProperty.Register(nameof(EditCommand), typeof(ICommand), typeof(ActionButton));

    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(ActionButton));

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

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion

    private void DotsButton_Click(object sender, RoutedEventArgs e)
    {
        ActionPopup.IsOpen = true;
        DotsButton.Visibility = Visibility.Hidden;

        // Scale animatsiya
        var scaleAnim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        stackScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleAnim);
        stackScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleAnim);

        // Translate animatsiya
        var moveAnim = new DoubleAnimation
        {
            From = 20,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        stackTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, moveAnim);
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        NotificationService.Show("Ishlamoqda", Common.Enums.NotificationType.Warning);
        var cmd = EditCommand;
        var param = CommandParameter ?? DataContext; // fallback DataContext
        if (cmd != null && cmd.CanExecute(param))
            cmd.Execute(param);

        ActionPopup.IsOpen = false;
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        NotificationService.Show("Ishlamoqda", Common.Enums.NotificationType.Warning);
        var cmd = DeleteCommand;
        var param = CommandParameter ?? DataContext; // fallback DataContext
        if (cmd != null && cmd.CanExecute(param))
            cmd.Execute(param);

        ActionPopup.IsOpen = false;
    }

}
