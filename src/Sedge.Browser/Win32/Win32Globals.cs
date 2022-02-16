namespace Sedge.Browser.Win32;

public delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

public enum KeyboardConstants
{
    WM_KEYDOWN = 0x100,
    WM_KEYUP = 257,
    WM_SYSKEYUP = 261,
    WM_SYSKEYDOWN = 260
}

public enum MouseMessages
{
    WM_LBUTTONDOWN = 513,
    WM_LBUTTONUP = 514,
    WM_MOUSEMOVE = 0x200,
    WM_MOUSEWHEEL = 522,
    WM_RBUTTONDOWN = 516,
    WM_RBUTTONUP = 517
}

public enum DWMWINDOWATTRIBUTE
{
    DWMWA_WINDOW_CORNER_PREFERENCE = 33
}

public enum DWM_WINDOW_CORNER_PREFERENCE
{
    DWMWCP_DEFAULT = 0,
    DWMWCP_DONOTROUND = 1,
    DWMWCP_ROUND = 2,
    DWMWCP_ROUNDSMALL = 3
}

[StructLayout(LayoutKind.Sequential)]
public struct OsVersionInfo
{
    public uint OsVersionInfoSize { get; }

    public uint MajorVersion { get; }
    public uint MinorVersion { get; }

    public uint BuildNumber { get; }

    public uint PlatformId { get; }

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    private readonly string _csdVersion;

    public string CSDVersion { get => _csdVersion; }
}

internal static class Win32Globals
{
    internal const int WH_KEYBOARD_LL = 13;
    internal const int WH_MOUSE_LL = 14;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern int AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern int FreeConsole();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetActiveWindow();

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern long DwmSetWindowAttribute(IntPtr hwnd,
        DWMWINDOWATTRIBUTE attribute,
        ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
        uint cbAttribute);

    [DllImport("ntdll.dll", SetLastError = true)]
    internal static extern uint RtlGetVersion(out OsVersionInfo versionInformation);

    [DllImport("winbrand.dll", CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern string BrandingFormatString(string format);
}
