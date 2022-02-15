namespace Sedge.Browser.Hooks;

public class HookEventArgs : EventArgs
{
    public HookEventSource Source { get; }
    public MouseButtons MouseButton { get; }
    public Keys Key { get; }

    internal HookEventArgs(HookEventSource source, MouseButtons mouseButton, Keys key)
    {
        Source = source;
        MouseButton = mouseButton;
        Key = key;
    }
}

public enum HookEventSource
{
    Keyboard =  0x0000,
    Mouse =     0x0002
}