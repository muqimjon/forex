namespace Forex.Wpf.Common.Services;

using Forex.Wpf.Common.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

public static class NotificationService
{
    private static readonly List<Grid> _activeNotifications = [];
    private static readonly object _lock = new();

    public static void Show(
        string message,
        NotificationType type = NotificationType.Error,
        NotificationPosition position = NotificationPosition.BottomRight,
        double speedMultiplier = 1.0,
        double opacity = 0.9,
        double widthPercentage = 33.0)
    {
        if (Application.Current.MainWindow is not Window mainWindow)
            return;

        var rootGrid = TryFindRootGrid(mainWindow);
        if (rootGrid == null)
            return;

        // 📏 Calculate duration based on message length
        var durationSeconds = CalculateReadingDuration(message, speedMultiplier);

        var background = GetBackground(type);
        var darkerBackground = GetDarkerBrush(background);

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
            Text = message,
            Foreground = Brushes.White,
            FontSize = 16,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.WordEllipsis,
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

        // 🎯 Set initial width based on window size and percentage
        UpdateContainerWidth(container, mainWindow, widthPercentage);

        container.Children.Add(messageBorder);

        // 🔄 Shift existing notifications up
        lock (_lock)
        {
            ShiftNotificationsUp(position);
            _activeNotifications.Add(container);
        }

        rootGrid.Children.Add(container);

        // 📐 Handle window size changes
        SizeChangedEventHandler sizeHandler = null!;
        sizeHandler = (s, e) =>
        {
            UpdateContainerWidth(container, mainWindow, widthPercentage);
        };
        mainWindow.SizeChanged += sizeHandler;

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
            cornerAnim.KeyFrames.Add(new DiscreteObjectKeyFrame(new CornerRadius(1, 2, 4, 8), KeyTime.FromPercent(0.09)));
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
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.Children.Add(cornerAnim);
            storyboard.Children.Add(opacityAnim);
            storyboard.Begin();

            // ✅ Snackbar fade out
            Task.Delay((int)(durationSeconds * 1000)).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
                    fadeOut.Completed += (_, _) =>
                    {
                        mainWindow.SizeChanged -= sizeHandler;
                        rootGrid.Children.Remove(container);

                        lock (_lock)
                        {
                            _activeNotifications.Remove(container);
                        }
                    };
                    container.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                });
            });
        };
    }

    /// <summary>
    /// Calculates reading duration based on message length
    /// Average reading speed: 200-250 words per minute (3-4 words per second)
    /// </summary>
    private static double CalculateReadingDuration(string message, double speedMultiplier)
    {
        if (string.IsNullOrWhiteSpace(message))
            return 3.0 / speedMultiplier;

        // Count words
        var wordCount = message.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;

        // Base reading speed: 3.5 words per second (average comfortable reading)
        var wordsPerSecond = 4d;

        // Calculate base duration
        var baseDuration = wordCount / wordsPerSecond;

        // Add minimum time (2 seconds) and maximum time (15 seconds)
        var minDuration = 2d;
        var maxDuration = 15d;

        baseDuration = Math.Max(minDuration, Math.Min(baseDuration, maxDuration));

        // Apply speed multiplier (1.0 = normal, 2.0 = 2x faster, 0.5 = 2x slower)
        return baseDuration / speedMultiplier;
    }

    /// <summary>
    /// Shifts existing notifications upward to make room for new ones
    /// </summary>
    private static void ShiftNotificationsUp(NotificationPosition position)
    {
        if (_activeNotifications.Count == 0)
            return;

        foreach (var notification in _activeNotifications)
        {
            if (notification.VerticalAlignment != GetVerticalAlignment(position) ||
                notification.HorizontalAlignment != GetHorizontalAlignment(position))
                continue;

            // Get current margin
            var currentMargin = notification.Margin;

            // Calculate new vertical offset based on notification height
            var notificationHeight = notification.ActualHeight > 0 ? notification.ActualHeight : 60; // Estimate if not rendered
            var spacing = 10; // Space between notifications
            var offset = notificationHeight + spacing;

            // Apply offset based on position
            Thickness newMargin;
            if (position == NotificationPosition.TopLeft || position == NotificationPosition.TopRight)
            {
                // Top positions: shift down
                newMargin = new Thickness(
                    currentMargin.Left,
                    currentMargin.Top + offset,
                    currentMargin.Right,
                    currentMargin.Bottom
                );
            }
            else
            {
                // Bottom positions: shift up
                newMargin = new Thickness(
                    currentMargin.Left,
                    currentMargin.Top,
                    currentMargin.Right,
                    currentMargin.Bottom + offset
                );
            }

            // Animate the shift
            var marginAnim = new ThicknessAnimation
            {
                From = currentMargin,
                To = newMargin,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            notification.BeginAnimation(FrameworkElement.MarginProperty, marginAnim);
        }
    }

    private static void UpdateContainerWidth(Grid container, Window window, double widthPercentage)
    {
        var calculatedWidth = window.ActualWidth * (widthPercentage / 100.0);

        // Minimum width to prevent too small notifications
        var minWidth = 200.0;
        var maxWidth = window.ActualWidth - 40; // 20px margin on each side

        container.Width = Math.Max(minWidth, Math.Min(calculatedWidth, maxWidth));
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