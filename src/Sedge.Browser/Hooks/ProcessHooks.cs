namespace Sedge.Browser.Hooks;

public class ProcessHooks : IProcessHooks
{
    public bool IsDisposed { get; private set; } = false;

    public event EventHandler<HookEventArgs>? Hooked;

    private readonly ICaptainLogger _logger;

    private readonly LowLevelProc _keybProc;
    private readonly LowLevelProc _mouseProc;

    private IntPtr _keybHookId = IntPtr.Zero;
    private IntPtr _mouseHookId = IntPtr.Zero;

    private DateTime _lastKeyb = DateTime.Now;
    private DateTime _lastMouse = DateTime.Now;

    public ProcessHooks(ICaptainLogger<ProcessHooks> logger)
    {
        _logger = logger;
        _keybProc = KeybHookCallback;
        _mouseProc = MouseHookCallback;

        SetHooks(_keybProc, _mouseProc);

        _logger.InformationLog($"Keyboard {_keybHookId}, and mouse {_mouseHookId} hooks created!");
    }

    ~ProcessHooks() => Dispose(false);

    public bool IsActiveWindow(IntPtr formHandle) => GetActiveWindow() == formHandle;

    private void SetHooks(LowLevelProc keybProc, LowLevelProc mouseProc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        if (curModule is null)
            return;

        _keybHookId = SetWindowsHookEx(WH_KEYBOARD_LL, keybProc
            , GetModuleHandle($"{curModule.ModuleName}"), 0);

        _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, mouseProc
           , GetModuleHandle($"{curModule.ModuleName}"), 0);
    }

    private IntPtr KeybHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _lastKeyb < DateTime.Now.AddMilliseconds(-10))
        {
            var keyAction = (KeyboardConstants)wParam;
            if (keyAction == KeyboardConstants.WM_KEYDOWN ||
                keyAction == KeyboardConstants.WM_SYSKEYDOWN)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                Hooked?.Invoke(this
                    , new HookEventArgs(HookEventSource.Keyboard
                    , MouseButtons.None
                    , (Keys)vkCode));

                _lastKeyb = DateTime.Now;
            }
        }

        return CallNextHookEx(_keybHookId, nCode, wParam, lParam);
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _lastMouse < DateTime.Now.AddMilliseconds(-10))
        {
            var mouseAction = (MouseMessages)wParam;
            if (mouseAction == MouseMessages.WM_LBUTTONDOWN ||
                mouseAction == MouseMessages.WM_RBUTTONDOWN)
            {
                var mb = MouseButtons.Left;
                if (mouseAction == MouseMessages.WM_RBUTTONDOWN)
                    mb = MouseButtons.Right;

                Hooked?.Invoke(this
                    , new HookEventArgs(HookEventSource.Mouse
                    , mb
                    , Keys.None));

                _lastMouse = DateTime.Now;
            }
        }

        return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
    }

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            //No private managed members
        }

        UnhookWindowsHookEx(_keybHookId);
        UnhookWindowsHookEx(_mouseHookId);

        _logger.InformationLog($"Keyboard {_keybHookId}, and mouse {_mouseHookId} hooks disposed!");

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
