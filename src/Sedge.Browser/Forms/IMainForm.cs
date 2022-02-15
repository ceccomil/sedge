namespace Sedge.Browser.Forms;

public interface IMainForm : IDisposable
{
    ICaptainLogger Logger { get; }

    SedgeBrowserOptions Options { get; }
    BoxButton BoxClose { get; }
    BoxButton BoxMinMax { get; }
    BoxButton BoxIcon { get; }
    Label Clock { get; }
    FormTimer ClockTimer { get; }
    Label StatusLabel { get; }
    WebView2 Browser { get; }
    FlatButton ShowNavigate { get; }
    UrlNavigation Navigation { get; }

    ICollection<string> CustomUserAgentFilters { get; }

    string? DefaultUserAgent { get; set; }

    FormWindowState WindowState { get; set; }
    Control.ControlCollection Controls { get; }

    int Width { get; set; }
    int Height { get; set; }
    int Top { get; }
    int Bottom { get; }
    int Left { get; }
    int Right { get; }

    void Close();
}
