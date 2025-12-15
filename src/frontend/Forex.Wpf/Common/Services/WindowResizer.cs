namespace Forex.Wpf.Common.Services;

using Forex.Wpf.Common.Enums;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

public static class WindowResizer
{
    #region Private Classes

    private class ResizeState
    {
        public double TargetWidth { get; set; }
        public double TargetHeight { get; set; }
        public bool HasResized { get; set; }
        public ResizeBehavior Behavior { get; set; }
    }

    #endregion

    #region Private Fields

    private static readonly Dictionary<Window, Dictionary<object, ResizeState>> States = new();
    private static readonly Lock _lock = new();

    #endregion

    #region Extension Methods

    /// <summary>
    /// Page/UserControl uchun Window ni avtomatik resize qiladi
    /// </summary>
    public static void ResizeWindow(
        this FrameworkElement element,
        double width,
        double height,
        ResizeBehavior behavior = ResizeBehavior.Once,
        double duration = 0.4,
        bool center = true)
    {
        if (element is null)
            return;

        // Dispatcher orqali UI thread da ishlash
        element.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (element.IsLoaded)
            {
                PerformResize(element, width, height, behavior, duration, center);
            }
            else
            {
                void handler(object? s, RoutedEventArgs e)
                {
                    element.Loaded -= handler;
                    // Biroz kechiktirib ishga tushirish (rendering tugashi uchun)
                    element.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        PerformResize(element, width, height, behavior, duration, center);
                    }), DispatcherPriority.Loaded);
                }
                element.Loaded += handler;
            }
        }), DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Window uchun to'g'ridan-to'g'ri resize
    /// </summary>
    public static void ResizeWindow(
        this Window window,
        double width,
        double height,
        object? context = null,
        ResizeBehavior behavior = ResizeBehavior.Once,
        double duration = 0.4,
        bool center = true)
    {
        if (window is null)
            return;

        context ??= window;

        window.Dispatcher.BeginInvoke(new Action(() =>
        {
            AnimateResize(window, width, height, context, behavior, duration, center);
        }), DispatcherPriority.Loaded);
    }

    #endregion

    #region Private Methods

    private static void PerformResize(
        FrameworkElement element,
        double width,
        double height,
        ResizeBehavior behavior,
        double duration,
        bool center)
    {
        var window = FindWindow(element);
        if (window is null)
            return;

        AnimateResize(window, width, height, element, behavior, duration, center);
    }

    private static void AnimateResize(
        Window window,
        double targetWidth,
        double targetHeight,
        object context,
        ResizeBehavior behavior,
        double duration,
        bool center)
    {
        if (targetWidth <= 0 || targetHeight <= 0)
            return;

        lock (_lock)
        {
            if (!States.TryGetValue(window, out var contextStates))
            {
                contextStates = new Dictionary<object, ResizeState>();
                States[window] = contextStates;
                SetupCleanup(window);
            }

            if (!contextStates.TryGetValue(context, out var state))
            {
                state = new ResizeState
                {
                    TargetWidth = targetWidth,
                    TargetHeight = targetHeight,
                    HasResized = false,
                    Behavior = behavior
                };
                contextStates[context] = state;
            }

            // Once rejimida va allaqachon resize qilingan bo'lsa
            if (state.Behavior == ResizeBehavior.Once && state.HasResized)
                return;

            // Faqat kichik bo'lsa resize qilish
            double currentWidth = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
            double currentHeight = window.ActualHeight > 0 ? window.ActualHeight : window.Height;

            bool needsResize = currentWidth < targetWidth || currentHeight < targetHeight;

            if (!needsResize && state.HasResized)
                return;

            ExecuteAnimation(window, targetWidth, targetHeight, duration, center);
            state.HasResized = true;
        }
    }

    private static void ExecuteAnimation(
        Window window,
        double targetWidth,
        double targetHeight,
        double duration,
        bool center)
    {
        // Hozirgi o'lchamlarni olish
        double currentWidth = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
        double currentHeight = window.ActualHeight > 0 ? window.ActualHeight : window.Height;
        double currentLeft = window.Left;
        double currentTop = window.Top;

        // Agar current va target bir xil bo'lsa, chiqib ketamiz
        if (Math.Abs(currentWidth - targetWidth) < 1 && Math.Abs(currentHeight - targetHeight) < 1)
            return;

        // WindowStartupLocation ni Manual ga o'zgartirish
        if (window.WindowStartupLocation != WindowStartupLocation.Manual)
            window.WindowStartupLocation = WindowStartupLocation.Manual;

        // SizeToContent ni o'chirish
        window.SizeToContent = SizeToContent.Manual;

        var animDuration = new Duration(TimeSpan.FromSeconds(duration));
        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

        // Width animatsiya
        var widthAnimation = new DoubleAnimation
        {
            From = currentWidth,
            To = targetWidth,
            Duration = animDuration,
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop
        };

        // Height animatsiya
        var heightAnimation = new DoubleAnimation
        {
            From = currentHeight,
            To = targetHeight,
            Duration = animDuration,
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop
        };

        // Animatsiya tugagach qiymatni o'rnatish
        widthAnimation.Completed += (s, e) =>
        {
            window.BeginAnimation(FrameworkElement.WidthProperty, null);
            window.Width = targetWidth;
        };

        heightAnimation.Completed += (s, e) =>
        {
            window.BeginAnimation(FrameworkElement.HeightProperty, null);
            window.Height = targetHeight;
        };

        // Markazlash
        if (center)
        {
            double deltaWidth = targetWidth - currentWidth;
            double deltaHeight = targetHeight - currentHeight;

            double targetLeft = currentLeft - deltaWidth / 2;
            double targetTop = currentTop - deltaHeight / 2;

            // Ekrandan chiqib ketmasligini ta'minlash
            var screen = System.Windows.Forms.Screen.FromHandle(
                new System.Windows.Interop.WindowInteropHelper(window).Handle);

            if (screen != null)
            {
                double maxLeft = screen.WorkingArea.Right - targetWidth;
                double maxTop = screen.WorkingArea.Bottom - targetHeight;

                targetLeft = Math.Max(screen.WorkingArea.Left, Math.Min(targetLeft, maxLeft));
                targetTop = Math.Max(screen.WorkingArea.Top, Math.Min(targetTop, maxTop));
            }

            var leftAnimation = new DoubleAnimation
            {
                From = currentLeft,
                To = targetLeft,
                Duration = animDuration,
                EasingFunction = easing,
                FillBehavior = FillBehavior.Stop
            };

            var topAnimation = new DoubleAnimation
            {
                From = currentTop,
                To = targetTop,
                Duration = animDuration,
                EasingFunction = easing,
                FillBehavior = FillBehavior.Stop
            };

            leftAnimation.Completed += (s, e) =>
            {
                window.BeginAnimation(Window.LeftProperty, null);
                window.Left = targetLeft;
            };

            topAnimation.Completed += (s, e) =>
            {
                window.BeginAnimation(Window.TopProperty, null);
                window.Top = targetTop;
            };

            window.BeginAnimation(Window.LeftProperty, leftAnimation);
            window.BeginAnimation(Window.TopProperty, topAnimation);
        }

        window.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
        window.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
    }

    private static Window? FindWindow(DependencyObject element)
    {
        // Window.GetWindow - eng tezkor usul
        if (element is FrameworkElement fe)
        {
            var window = Window.GetWindow(fe);
            if (window != null)
                return window;
        }

        // Visual tree orqali qidirish
        var visualWindow = FindVisualParent<Window>(element);
        if (visualWindow != null)
            return visualWindow;

        // Logical tree orqali qidirish
        DependencyObject current = element;
        while (current != null)
        {
            if (current is Window win)
                return win;
            current = LogicalTreeHelper.GetParent(current);
        }

        return null;
    }

    private static T? FindVisualParent<T>(DependencyObject element) where T : DependencyObject
    {
        while (element != null)
        {
            if (element is T parent)
                return parent;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    private static void SetupCleanup(Window window)
    {
        void handler(object? sender, EventArgs e)
        {
            window.Closed -= handler;
            lock (_lock)
            {
                States.Remove(window);
            }
        }
        window.Closed += handler;
    }

    #endregion

    #region Public Utility Methods

    /// <summary>
    /// Context ni reset qilish
    /// </summary>
    public static void ResetContext(FrameworkElement element)
    {
        var window = FindWindow(element);
        if (window == null)
            return;

        lock (_lock)
        {
            if (States.TryGetValue(window, out var contextStates))
            {
                contextStates.Remove(element);
            }
        }
    }

    /// <summary>
    /// Barcha holatlarni tozalash
    /// </summary>
    public static void ClearAll()
    {
        lock (_lock)
        {
            States.Clear();
        }
    }

    #endregion
}