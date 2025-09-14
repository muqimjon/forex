namespace Forex.Wpf.Services;

using System.Windows;
using System.Windows.Media.Animation;

public static class WindowResizer
{
    /// <summary>
    /// MainWindow ni animatsiya bilan kerakli o‘lchamga o‘tkazadi
    /// </summary>
    public static void AnimateToSize(Window window, double targetWidth, double targetHeight, double seconds = 0.8)
    {
        if (window == null) return;

        if (window.Width < targetWidth || window.Height < targetHeight)
        {
            var duration = TimeSpan.FromSeconds(seconds);
            var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            double deltaWidth = targetWidth - window.Width;
            double deltaHeight = targetHeight - window.Height;

            var leftAnimation = new DoubleAnimation
            {
                To = window.Left - deltaWidth / 2,
                Duration = duration,
                EasingFunction = easing
            };

            var topAnimation = new DoubleAnimation
            {
                To = window.Top - deltaHeight / 2,
                Duration = duration,
                EasingFunction = easing
            };

            var widthAnimation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = duration,
                EasingFunction = easing
            };

            var heightAnimation = new DoubleAnimation
            {
                To = targetHeight,
                Duration = duration,
                EasingFunction = easing
            };

            window.BeginAnimation(Window.LeftProperty, leftAnimation);
            window.BeginAnimation(Window.TopProperty, topAnimation);
            window.BeginAnimation(Window.WidthProperty, widthAnimation);
            window.BeginAnimation(Window.HeightProperty, heightAnimation);
        }
    }
}
