﻿namespace Sedge.Browser.Forms;

[DesignerCategory("Code")]
public class BrowserForm : Form, IBrowserForm
{
    private readonly IContainer _components;
    private readonly IDrawBorders _drawBorders;
    private readonly IProcessHooks _hooks;
    private readonly IBrowsersList _urls;

    private bool _isLoaded = false;
    private bool _isResizing = false;

    public ICaptainLogger Logger { get; }

    public CoreWebView2Deferral? Deferral { get; set; }
    public CoreWebView2NewWindowRequestedEventArgs? NewWindowArgs { get; set; }

    public SedgeBrowserOptions Options { get; }
    public IBrowserEnv EnvService { get; }
    public IBrowserFormCollection BrowserForms { get; }

    public bool IsMainForm => BrowserForms.MainForm == this;

    public BoxButton BoxClose { get; } = new(BoxButtons.Close);
    public BoxButton BoxMinMax { get; } = new(BoxButtons.Maximize);
    public BoxButton BoxIcon { get; } = new(BoxButtons.Icon);

    public string Title
    {
        get => Text;
        set => Text = value;
    }

    public Label Clock { get; } = new();
    public FormTimer ClockTimer { get; } = new();

    public Label StatusLabel { get; } = new();

    public WebView2 Browser { get; } = new WebView2();

    public FlatButton ShowNavigate { get; } = new();

    public IUrlNavigation Navigation { get; }

    public ICollection<string> CustomUserAgentFilters { get; } = null!;

    public string? DefaultUserAgent { get; set; }

    public Color CurrentBorderColor => IsMainForm ? BorderAndStatus : BorderAndStatusChildren;
    public Color CurrentBackColor => IsMainForm ? DarkPanel : DarkPanelChildren;

    public BrowserForm(
        ICaptainLogger<BrowserForm> logger,
        IDrawBorders drawBorders,
        SedgeBrowserOptions opts,
        IBrowserEnv browserEnv,
        IEnumerable<string> filters,
        IProcessHooks hooks,
        IBrowserFormCollection browserForms,
        ICaptainLogger<UrlNavigation> urlNavigationLogger,
        IBrowsersList urls)
    {
        Logger = logger;
        _components = new Container();
        _drawBorders = drawBorders;
        Options = opts;
        EnvService = browserEnv;

        Opacity = 0.0d;

        CustomUserAgentFilters = new List<string>(filters);
        _hooks = hooks;
        BrowserForms = browserForms;

        Navigation = new UrlNavigation(
            this,
            urlNavigationLogger);

        _urls = urls;

        Init();
    }

    ~BrowserForm() => Dispose(false);

    private void Init()
    {
        SuspendLayout();
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Icon = Properties.Resources.SedgeIcon;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new(600, 450);
        Text = nameof(BrowserForm);

        if (!Options.IsShared)
            ShowNavigate.Visible = false;

        ResumeLayout(false);

        Load += async (o, e) =>
        {
            this.SetupUrlNavigation();
            this.SetupBoxButtons();
            this.SetupShowNavigate();
            this.SetupStatusBar();

            await Task.Delay(1000);

            this.SetLocation();

            await this.SetupBrowser(Options.StartUrl.AbsoluteUri);

            if (IsMainForm)
            {
                Logger.InformationLog(
                    $"Application is started: {Options.StartUrl} " +
                    $"- userData: {Options.UserData}");

                foreach (var url in _urls.Urls)
                {
                    Logger.InformationLog($"Url: {url}");
                    Navigation.GoToUrlNewWindow(url);
                }
            }

            Opacity = 1.0d;
            _isLoaded = true;
            Invalidate();
        };

        FormClosing += async (o, e) =>
        {
            if (!IsMainForm)
            {
                return;
            }

            await Settings.SaveSettings(Options.WindowSettings);
        };

        ResizeEnd += (o, e) =>
        {
            if (!_isLoaded || !IsMainForm)
                return;

            if (WindowState == FormWindowState.Normal)
            {
                Options.WindowSettings.Width = ClientSize.Width;
                Options.WindowSettings.Height = ClientSize.Height;
            }
        };

        LocationChanged += (o, e) =>
        {
            if (!_isLoaded || !IsMainForm)
                return;

            if (WindowState == FormWindowState.Normal)
            {
                Options.WindowSettings.X = Left;
                Options.WindowSettings.Y = Top;
            }
        };

        _hooks.Hooked += (o, e) =>
        {
            if (IsDisposed || Disposing)
                return;

            if (!_hooks.IsActiveWindow(Handle))
                return;

            if (e.Source != HookEventSource.Keyboard)
                return;

            if (Navigation.IsVisible && e.Key == Keys.Escape)
                Navigation.ToggleShow();

            if (Navigation.IsVisible)
                return;

            if (ModifierKeys.HasFlag(Keys.Control) && e.Key == Keys.N)
                ShowNavigate.Visible = !ShowNavigate.Visible;

            if (ModifierKeys.HasFlag(Keys.Control) && e.Key == Keys.U)
            {
                Navigation.HideBtnOnClose = !ShowNavigate.Visible;
                Navigation.Url = StatusLabel.Text;
                ShowNavigate.Visible = true;
                Navigation.ToggleShow();
            }
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _components is not null)
            _components.Dispose();

        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_isResizing)
        {
            e.Graphics.Clear(CurrentBackColor);
            e.Graphics.FillRectangle(new SolidBrush(CurrentBorderColor), 0, Height - 26, Width, 26);

            _drawBorders
                .DrawRoundCornerAndBorder(this, e.Graphics, CurrentBorderColor);
        }
    }

    protected override void WndProc(ref Message m)
    {
        //const int WM_ACTIVATE = 0x0006;

        const int
            CCAPTION = 32, // Caption bar height
            WM_NCHITTEST = 0x84,
            HTCAPTION = 2,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17;

        if (m.Msg == WM_NCHITTEST)
        {
            var pos = new Point((int)m.LParam);
            pos = PointToClient(pos);

            if (pos.X >= 0 && pos.X <= 16 && pos.Y <= 16)
            {
                _isResizing = true;
                m.Result = (IntPtr)HTTOPLEFT;
                return;
            }
            else if (pos.X >= ClientRectangle.Right - 16 && pos.Y <= 16)
            {
                _isResizing = true;
                m.Result = (IntPtr)HTTOPRIGHT;
                return;
            }
            else if (pos.X >= 0 && pos.X <= 16 && pos.Y >= ClientRectangle.Bottom - 16)
            {
                _isResizing = true;
                m.Result = (IntPtr)HTBOTTOMLEFT;
                return;
            }
            else if (pos.X >= ClientRectangle.Right - 16 && pos.Y >= ClientRectangle.Bottom - 16)
            {
                _isResizing = true;
                m.Result = (IntPtr)HTBOTTOMRIGHT;
                return;
            }
            else if (pos.X >= 0 && pos.X <= 16)
            {
                _isResizing = true;
                m.Result = (IntPtr)HTLEFT;
                return;
            }
            else if (pos.Y <= 4)
            {
                _isResizing = true;
                m.Result = (IntPtr)HTTOP;
                return;
            }
            else if (pos.X >= ClientRectangle.Right - 16)
            {
                _isResizing = true;
                m.Result = (IntPtr)HTRIGHT;
                return;
            }
            else if (pos.Y >= ClientRectangle.Bottom - 16)
            {
                _isResizing = true;
                m.Result = (IntPtr)HTBOTTOM;
                return;
            }
            else if (pos.Y < CCAPTION)
            {
                m.Result = (IntPtr)HTCAPTION;
                return;
            }
        }

        _isResizing = false;

        base.WndProc(ref m);
    }
}
