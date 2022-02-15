namespace Sedge.Browser.Hooks;

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

internal static class Win32Globals
{
    internal const int WH_KEYBOARD_LL = 13;
    internal const int WH_MOUSE_LL = 14;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetActiveWindow();
}
