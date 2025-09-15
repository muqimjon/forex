namespace Forex.Wpf.Services;

using Forex.Wpf.Enums;
using Forex.Wpf.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;

public static class NotificationService
{
    public static void Show(
        string message,
        NotificationType type = NotificationType.Error,
        NotificationPosition position = NotificationPosition.BottomRight,
        int durationSeconds = 5,
        double opacity = 0.7,
        int maxLineLength = 40)
    {
        if (Application.Current.MainWindow is not Window mainWindow)
            return;

        var rootGrid = TryFindRootGrid(mainWindow);
        if (rootGrid == null)
            return;

        var background = GetBackground(type);
        var darkerBackground = GetDarkerBrush(background);

        // ✂️ Wrap message
        var wrappedMessage = message.WrapWithNewLines(maxLineLength);

        // 📦 Snackbar content with embedded timeline
        var dock = new DockPanel { LastChildFill = true };

        var progressBar = new Border
        {
            Height = 5,
            Background = darkerBackground,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            CornerRadius = new CornerRadius(1, 1, 8, 8)
        };
        DockPanel.SetDock(progressBar, Dock.Bottom);

        var messageText = new TextBlock
        {
            Text = wrappedMessage,
            Foreground = Brushes.White,
            FontSize = 16,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(15, 10, 15, 10)
        };

        dock.Children.Add(progressBar);
        dock.Children.Add(messageText);

        var messageBorder = new Border
        {
            Background = background,
            CornerRadius = new CornerRadius(8),
            Opacity = opacity,
            Child = dock
        };

        var container = new Grid
        {
            Margin = new Thickness(10),
            IsHitTestVisible = false,
            VerticalAlignment = GetVerticalAlignment(position),
            HorizontalAlignment = GetHorizontalAlignment(position),
            Opacity = 0
        };
        container.Children.Add(messageBorder);
        rootGrid.Children.Add(container);

        // 🎞️ Fade in
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
        container.BeginAnimation(UIElement.OpacityProperty, fadeIn);

        // 🎞️ Timeline progress + fade out
        messageBorder.Loaded += (_, _) =>
        {
            var fullWidth = messageBorder.ActualWidth;
            progressBar.Width = fullWidth;

            var widthAnim = new DoubleAnimation
            {
                From = fullWidth,
                To = 0,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            progressBar.BeginAnimation(FrameworkElement.WidthProperty, widthAnim);

            var cornerAnim = new ObjectAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromSeconds(durationSeconds)
            };

            cornerAnim.KeyFrames.Add(new DiscreteObjectKeyFrame(new CornerRadius(1, 1, 7, 8), KeyTime.FromPercent(0.03)));
            cornerAnim.KeyFrames.Add(new DiscreteObjectKeyFrame(new CornerRadius(1, 1, 6, 8), KeyTime.FromPercent(0.05)));
            cornerAnim.KeyFrames.Add(new DiscreteObjectKeyFrame(new CornerRadius(1, 2, 5, 8), KeyTime.FromPercent(0.07)));
            cornerAnim.KeyFrames.Add(new DiscreteObjectKeyFrame(new CornerRadius(1, 2, 4, 8), KeyTime.FromPercent(0.9)));
            cornerAnim.KeyFrames.Add(new DiscreteObjectKeyFrame(new CornerRadius(1, 2, 3, 8), KeyTime.FromPercent(0.11)));
            cornerAnim.KeyFrames.Add(new DiscreteObjectKeyFrame(new CornerRadius(0, 2, 2, 8), KeyTime.FromPercent(0.13)));

            Storyboard.SetTarget(cornerAnim, progressBar);
            Storyboard.SetTargetProperty(cornerAnim, new PropertyPath(Border.CornerRadiusProperty));

            var opacityAnim = new DoubleAnimation
            {
                From = opacity,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(durationSeconds * 0.75),
                Duration = TimeSpan.FromSeconds(durationSeconds * 0.25),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(opacityAnim, progressBar);
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(Border.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.Children.Add(cornerAnim);
            storyboard.Children.Add(opacityAnim);
            storyboard.Begin();

            // ✅ Snackbar fade out
            System.Threading.Tasks.Task.Delay(durationSeconds * 1000).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
                    fadeOut.Completed += (_, _) => rootGrid.Children.Remove(container);
                    container.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                });
            });
        };
    }

    private static Grid? TryFindRootGrid(Window window)
    {
        if (window.Content is Grid grid)
            return grid;

        if (window.Content is Frame frame && frame.Content is Page page)
        {
            return VisualTreeHelper.GetChild(page, 0) as Grid;
        }

        return null;
    }

    private static SolidColorBrush GetBackground(NotificationType type) => type switch
    {
        NotificationType.Info => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
        NotificationType.Success => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
        NotificationType.Warning => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
        NotificationType.Error => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
        _ => new SolidColorBrush(Colors.Gray)
    };

    private static SolidColorBrush GetDarkerBrush(SolidColorBrush baseBrush)
    {
        var c = baseBrush.Color;
        static byte Darken(byte value) => (byte)Math.Max(0, value * 0.7);
        return new SolidColorBrush(Color.FromRgb(Darken(c.R), Darken(c.G), Darken(c.B)));
    }

    private static VerticalAlignment GetVerticalAlignment(NotificationPosition position) => position switch
    {
        NotificationPosition.TopLeft or NotificationPosition.TopRight => VerticalAlignment.Top,
        NotificationPosition.Center => VerticalAlignment.Center,
        _ => VerticalAlignment.Bottom
    };

    private static HorizontalAlignment GetHorizontalAlignment(NotificationPosition position) => position switch
    {
        NotificationPosition.TopLeft or NotificationPosition.BottomLeft => HorizontalAlignment.Left,
        NotificationPosition.Center => HorizontalAlignment.Center,
        _ => HorizontalAlignment.Right
    };
}
