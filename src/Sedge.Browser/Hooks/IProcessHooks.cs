namespace Sedge.Browser.Hooks;

public interface IProcessHooks : IDisposable
{
    bool IsDisposed { get; }

    event EventHandler<HookEventArgs>? Hooked;

    bool IsActiveWindow(IntPtr formHandle);
}
