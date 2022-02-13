namespace Sedge.Browser.Forms;

public interface IMainForm : IDisposable
{
    SedgeBrowserOptions Options { get; }
    BoxButton BoxClose { get; }
    BoxButton BoxMinMax { get; }
    BoxButton BoxIcon { get; }
    Label Clock { get; }
    FormTimer ClockTimer { get; }
    Label StatusLabel { get; }
    WebView2 Browser { get; }

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
