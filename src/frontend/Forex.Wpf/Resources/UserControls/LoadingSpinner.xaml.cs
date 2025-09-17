namespace Forex.Wpf.Resources.UserControls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

public partial class LoadingSpinner : UserControl
{
    private readonly Storyboard rotationStoryboard;
    private readonly DoubleAnimation rotationAnimation;

    public LoadingSpinner()
    {
        InitializeComponent();

        // Rotation animation (forever)
        rotationAnimation = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(1)))
        {
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = null
        };

        rotationStoryboard = new Storyboard();
        rotationStoryboard.Children.Add(rotationAnimation);

        // Target the RotateTransform (SpinnerRotate)
        Storyboard.SetTarget(rotationAnimation, SpinnerRotate);
        Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath("Angle"));
    }

    // DependencyProperty
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(LoadingSpinner),
            new PropertyMetadata(false, OnIsActiveChanged));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((LoadingSpinner)d).HandleIsActiveChanged((bool)e.NewValue);

    private void HandleIsActiveChanged(bool active)
    {
        // Ensure UI thread
        Dispatcher.Invoke(() =>
        {
            if (active)
                ShowSpinner();
            else
                HideSpinner();
        });
    }

    private void ShowSpinner()
    {
        // Make visible immediately, then fade-in
        Visibility = Visibility.Visible;
        IsHitTestVisible = false; // spinner shouldn't block clicks by default

        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        BeginAnimation(OpacityProperty, fadeIn);

        // Start rotation
        rotationStoryboard.Begin(this, true);
    }

    private void HideSpinner()
    {
        // Fade out, then collapse and stop rotation
        var fadeOut = new DoubleAnimation
        {
            From = Opacity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        fadeOut.Completed += (_, __) =>
        {
            rotationStoryboard.Stop(this);
            Visibility = Visibility.Collapsed;
        };

        BeginAnimation(OpacityProperty, fadeOut);
    }
}