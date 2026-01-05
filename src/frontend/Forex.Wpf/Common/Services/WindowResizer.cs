namespace Forex.Wpf.Common.Services;

using System.Windows;
using System.Windows.Media.Animation;

public static class WindowResizeService
{
    private sealed class State
    {
        public bool Applied;
        public bool Always;
    }

    private static readonly Dictionary<Window, Dictionary<object, State>> Cache = new();
    private static readonly object Sync = new();

    #region Code-behind API

    public static void ResizeWindow(
        this FrameworkElement source,
        double width,
        double height,
        bool always = false,
        bool lockSize = false,
        double duration = 0.35,
        bool center = true)
    {
        if (source == null || width <= 0 || height <= 0)
            return;

        void run()
        {
            var window = Window.GetWindow(source);
            if (window == null)
                return;

            Apply(window, source, width, height, always, lockSize, duration, center);
        }

        if (source.IsLoaded)
            run();
        else
            source.Loaded += (_, _) => run();
    }

    #endregion

    #region XAML API

    public static readonly DependencyProperty WidthProperty =
        Register("Width", 0d);

    public static readonly DependencyProperty HeightProperty =
        Register("Height", 0d);

    public static readonly DependencyProperty AlwaysProperty =
        Register("Always", false);

    public static readonly DependencyProperty LockProperty =
        Register("Lock", false);

    public static readonly DependencyProperty DurationProperty =
        Register("Duration", 0.35);

    public static readonly DependencyProperty CenterProperty =
        Register("Center", true);

    private static readonly DependencyProperty InitializedProperty =
        DependencyProperty.RegisterAttached(
            "Initialized",
            typeof(bool),
            typeof(WindowResizeService),
            new PropertyMetadata(false));

    public static double GetWidth(DependencyObject o) => (double)o.GetValue(WidthProperty);
    public static void SetWidth(DependencyObject o, double v) => o.SetValue(WidthProperty, v);

    public static double GetHeight(DependencyObject o) => (double)o.GetValue(HeightProperty);
    public static void SetHeight(DependencyObject o, double v) => o.SetValue(HeightProperty, v);

    public static bool GetAlways(DependencyObject o) => (bool)o.GetValue(AlwaysProperty);
    public static void SetAlways(DependencyObject o, bool v) => o.SetValue(AlwaysProperty, v);

    public static bool GetLock(DependencyObject o) => (bool)o.GetValue(LockProperty);
    public static void SetLock(DependencyObject o, bool v) => o.SetValue(LockProperty, v);

    public static double GetDuration(DependencyObject o) => (double)o.GetValue(DurationProperty);
    public static void SetDuration(DependencyObject o, double v) => o.SetValue(DurationProperty, v);

    public static bool GetCenter(DependencyObject o) => (bool)o.GetValue(CenterProperty);
    public static void SetCenter(DependencyObject o, bool v) => o.SetValue(CenterProperty, v);

    private static DependencyProperty Register(string name, object def) =>
        DependencyProperty.RegisterAttached(
            name,
            def.GetType(),
            typeof(WindowResizeService),
            new PropertyMetadata(def, OnAttachedChanged));

    private static void OnAttachedChanged(DependencyObject d, DependencyPropertyChangedEventArgs _)
    {
        if (d is not FrameworkElement e)
            return;

        e.Unloaded += (_, _) =>
        {
            var window = Window.GetWindow(e);
            if (window == null) return;

            lock (Sync)
                if (Cache.TryGetValue(window, out var map))
                    map.Remove(e);
        };

        if ((bool)e.GetValue(InitializedProperty))
            return;

        e.SetValue(InitializedProperty, true);

        e.Loaded += (_, _) =>
            e.ResizeWindow(
                GetWidth(e),
                GetHeight(e),
                GetAlways(e),
                GetLock(e),
                GetDuration(e),
                GetCenter(e));
    }

    #endregion

    #region Core

    private static void Apply(
        Window window,
        object ctx,
        double w,
        double h,
        bool always,
        bool lockSize,
        double duration,
        bool center)
    {
        lock (Sync)
        {
            if (!Cache.TryGetValue(window, out var map))
            {
                map = [];
                Cache[window] = map;
                window.Closed += (_, _) =>
                {
                    lock (Sync)
                        Cache.Remove(window);
                };
            }

            if (!map.TryGetValue(ctx, out var state))
                map[ctx] = state = new();

            state.Always = always;

            if (state.Applied && !state.Always)
                return;

            state.Applied = true;

            Animate(window, w, h, duration, center, lockSize);
        }
    }

    private static void Animate(
        Window window,
        double w,
        double h,
        double duration,
        bool center,
        bool lockSize)
    {
        var cw = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
        var ch = window.ActualHeight > 0 ? window.ActualHeight : window.Height;

        if (Math.Abs(cw - w) < 1 && Math.Abs(ch - h) < 1)
            return;

        window.SizeToContent = SizeToContent.Manual;
        window.WindowStartupLocation = WindowStartupLocation.Manual;

        if (lockSize)
        {
            window.ResizeMode = ResizeMode.NoResize;
            window.MinWidth = window.MaxWidth = w;
            window.MinHeight = window.MaxHeight = h;
        }
        else
        {
            window.ResizeMode = ResizeMode.CanResize;
            window.MinWidth = window.MinHeight = 0;
            window.MaxWidth = window.MaxHeight = double.PositiveInfinity;
        }

        Animate(window, FrameworkElement.WidthProperty, cw, w, duration);
        Animate(window, FrameworkElement.HeightProperty, ch, h, duration);

        if (!center)
            return;

        Animate(window, Window.LeftProperty, window.Left, window.Left - (w - cw) / 2, duration);
        Animate(window, Window.TopProperty, window.Top, window.Top - (h - ch) / 2, duration);
    }

    private static void Animate(
        UIElement target,
        DependencyProperty property,
        double from,
        double to,
        double seconds)
    {
        var anim = new DoubleAnimation(from, to, TimeSpan.FromSeconds(seconds))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        anim.Completed += (_, _) =>
        {
            target.BeginAnimation(property, null);
            target.SetValue(property, to);
        };

        target.BeginAnimation(property, anim);
    }

    #endregion
}
