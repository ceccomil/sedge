namespace Sedge.Browser.Helpers;

public class DrawBorders : IDrawBorders
{
    private readonly ICaptainLogger _logger;

    public DrawBorders(ICaptainLogger<DrawBorders> logger)
    {
        _logger = logger;
    }

    public void DrawRoundCornerAndBorder(Form form, Graphics g, Color color)
    {
        if (OsMajorVersion < 11 || !IsGpuCapable)
        {
            if (form.WindowState == FormWindowState.Normal)
                ControlPaint.DrawBorder(g, form.ClientRectangle, color, ButtonBorderStyle.Solid);

            return;
        }

        var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
        _ = DwmSetWindowAttribute(
            form.Handle,
            DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
            ref preference,
            sizeof(uint));
    }

    private int? _osMajor;
    public int OsMajorVersion
    {
        get
        {
            if (_osMajor.HasValue)
                return _osMajor.Value;

            _osMajor = SetOsMajorVersion();
            return _osMajor.Value;
        }
    }

    private bool? _isGpuCapable;
    public bool IsGpuCapable
    {
        get
        {
            if (_isGpuCapable.HasValue)
                return _isGpuCapable.Value;

            _isGpuCapable = IsGpuAvail(_logger);
            return _isGpuCapable.Value;
        }
    }

    private static int SetOsMajorVersion()
    {
        _ = RtlGetVersion(out OsVersionInfo ovi);
        var branding = BrandingFormatString("Windows branding: %WINDOWS_LONG%");

        if (ovi.BuildNumber >= 22000 && branding.Contains("11"))
            return 11;

        return (int)ovi.MajorVersion;
    }

    private static bool IsGpuAvail(ICaptainLogger logger)
    {
        var scope = new ManagementScope("\\\\.\\ROOT\\cimv2");
        var query = new ObjectQuery("SELECT * FROM Win32_VideoController");
        using var searcher = new ManagementObjectSearcher(scope, query);
        using var queryCollection = searcher.Get();

        var list = new List<string>();

        foreach (ManagementObject m in queryCollection)
        {
            var ac = $"{m.GetPropertyValue("AdapterCompatibility")}";
            var name = $"{m.GetPropertyValue("Name")}";
            logger.InformationLog($"Video adapter: {name}, compatibility: {ac}");

            if (!list.Contains(ac))
                list.Add(ac);
        }

        return list.Any(x => x != "Microsoft");
    }
}
