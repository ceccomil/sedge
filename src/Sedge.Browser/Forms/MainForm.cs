namespace Sedge.Browser.Forms;

[DesignerCategory("Code")]
public class MainForm : Form, IMainForm
{
    private readonly IContainer _components;
    private readonly IDrawBorders _drawBorders;
    private readonly IProcessHooks _hooks;

    private bool _isLoaded = false;
    private bool _isResizing = false;

    public ICaptainLogger Logger { get; }

    public SedgeBrowserOptions Options { get; }
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

    public UrlNavigation Navigation { get; } = new();

    public ICollection<string> CustomUserAgentFilters { get; } = null!;

    public string? DefaultUserAgent { get; set; }

    public MainForm(
        ICaptainLogger<MainForm> logger,
        IOptions<SedgeBrowserOptions> opts,
        IDrawBorders drawBorders,
        IConfiguration conf,
        IProcessHooks hooks)
    {
        Logger = logger;
        _components = new Container();
        _drawBorders = drawBorders;
        Options = opts.Value;
        Opacity = 0.0d;
        
        var filters = conf
            .GetSection("CustomUserAgentRequired")
            .Get<IEnumerable<string>>();

        CustomUserAgentFilters = new List<string>(filters);
        _hooks = hooks;
        Init();
    }

    ~MainForm() => Dispose(false);

    private void Init()
    {
        SuspendLayout();
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Icon = Properties.Resources.SedgeIcon;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new(600, 450);
        Text = nameof(MainForm);

        this.SetupUrlNavigation();
        this.SetupBoxButtons();
        this.SetupShowNavigate();
        this.SetupStatusBar();

        if (!Options.IsShared)
            ShowNavigate.Visible = false;

        ResumeLayout(false);

        Load += async (o, e) =>
        {
            Logger.InformationLog($"Application is started: {Options.StartUrl} - userData: {Options.UserData}");
            await Task.Delay(1000);

            Location = new(Options.WindowSettings.X, Options.WindowSettings.Y);
            ClientSize = new(Options.WindowSettings.Width, Options.WindowSettings.Height);
            if (Options.WindowSettings.IsMaximized)
                this.MinMaxForm();

            await this.SetupBrowser(Options.StartUrl.AbsoluteUri);

            Opacity = 1.0d;
            _isLoaded = true;
            Invalidate();
        };

        FormClosing += async (o, e) => await Settings.SaveSettings(Options.WindowSettings);

        ResizeEnd += (o, e) =>
        {
            if (!_isLoaded)
                return;

            if (WindowState == FormWindowState.Normal)
            {
                Options.WindowSettings.Width = ClientSize.Width;
                Options.WindowSettings.Height = ClientSize.Height;
            }
        };

        LocationChanged += (o, e) =>
        {
            if (!_isLoaded)
                return;

            if (WindowState == FormWindowState.Normal)
            {
                Options.WindowSettings.X = Left;
                Options.WindowSettings.Y = Top;
            }
        };

        _hooks.Hooked += (o, e) =>
        {
            if (!_hooks.IsActiveWindow(Handle))
                return;

            if (e.Source != HookEventSource.Keyboard)
                return;

            if (Navigation.Visible)
                return;

            if (ModifierKeys.HasFlag(Keys.Control) && e.Key == Keys.N)
                ShowNavigate.Visible = !ShowNavigate.Visible;
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
            e.Graphics.Clear(DarkPanel);
            e.Graphics.FillRectangle(new SolidBrush(BorderAndStatus), 0, Height - 26, Width, 26);

            _drawBorders
                .DrawRoundCornerAndBorder(this, e.Graphics, BorderAndStatus);
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
