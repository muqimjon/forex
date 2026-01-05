namespace Forex.Wpf.Common.Interfaces;

public interface IWindowSizeAware
{
    double WindowWidth { get; }
    double WindowHeight { get; }
    bool Always { get; }
    bool LockSize { get; }
    bool Center { get; }
}
