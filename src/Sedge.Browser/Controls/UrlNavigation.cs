namespace Sedge.Browser.Controls;

[DesignerCategory("Code")]
public class UrlNavigation : UserControl, IUrlNavigation
{
    private readonly TextBox _url = new();
    private readonly Button _navigate = new();
    private readonly IContainer _components = null!;

    public event EventHandler<NavEvArgs>? Navigate;

    public bool IsVisible => Visible;

    private readonly ICaptainLogger _logger;

    private readonly IBrowserForm _browserForm;

    public string Url
    {
        get => _url.Text;
        set => _url.Text = value;
    }

    public bool HideBtnOnClose { get; set; }

    public UrlNavigation(
        IBrowserForm browserForm,
        ICaptainLogger<UrlNavigation> logger)
    {
        _components = new Container();
        InitializeComponent();

        _navigate.Click += (o, e) => GoToUrl();
        _url.KeyDown += (o, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                GoToUrl();
            }
        };

        VisibleChanged += (o, e) => SelectUri();

        _browserForm = browserForm;
        _logger = logger;
    }

    ~UrlNavigation() => Dispose(false);

    public void ToggleShow()
    {
        Visible = !Visible;

        if (!Visible && HideBtnOnClose)
        {
            _browserForm.ShowNavigate.Visible = false;
            HideBtnOnClose = false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        _url.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _url.BorderStyle = BorderStyle.None;
        _url.Font = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        _url.Location = new(3, 6);
        _url.Name = "Url";
        _url.BackColor = DarkPanel;
        _url.ForeColor = YellowButton;
        _url.Size = new(594, 22);

        _navigate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _navigate.BackColor = DarkControl;
        _navigate.BackgroundImage = Properties.Resources.SendImg;
        _navigate.BackgroundImageLayout = ImageLayout.Stretch;
        _navigate.FlatAppearance.BorderSize = 0;
        _navigate.FlatAppearance.MouseDownBackColor = BlueBar;
        _navigate.FlatAppearance.MouseOverBackColor = LightDark;
        _navigate.FlatStyle = FlatStyle.Flat;
        _navigate.Location = new(554, 34);
        _navigate.Name = "Navigate";
        _navigate.Size = new(43, 43);
        _navigate.UseVisualStyleBackColor = true;

        Controls.Add(_navigate);
        Controls.Add(_url);
        MaximumSize = new(600, 80);
        MinimumSize = new(600, 80);
        Name = "UrlNavigation";
        Size = new(600, 80);
        Visible = false;
        ResumeLayout(false);
        PerformLayout();
    }

    private void SelectUri()
    {
        if (!Visible)
        {
            return;
        }

        _url.Focus();
        _url.Select();
        _url.SelectAll();
    }

    private void GoToUrl()
    {
        var txt = _url.Text;
        var domainPattern = @"^((?!-))(xn--)?[a-z0-9][a-z0-9-_]{0,61}[a-z0-9]{0,1}\.(xn--)?([a-z0-9\-]{1,61}|[a-z0-9-]{1,30}\.[a-z]{2,})$";

        var rgx = new Regex(domainPattern);
        if (!rgx.IsMatch(txt) && !txt.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            txt = _browserForm
                .BrowserForms
                .GetSearchUrl(txt);
        }

        if (!txt.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            txt = $"https://{txt}";
        }

        if (Uri.TryCreate(txt, UriKind.Absolute, out Uri? uri))
        {
            var newWindow = false;
            if (Ctrl.ModifierKeys == Keys.Control)
            {
                _logger.InformationLog("CTRL was pressed opening URL in a new window");
                newWindow = true;
            }

            Navigate?.Invoke(this, new(uri, newWindow));
            ToggleShow();
        }
    }

    public void GoToUrlNewWindow(string url)
    {
        Navigate?.Invoke(this, new(new Uri(url), true));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(DarkControl);
        ControlPaint.DrawBorder(e.Graphics, ClientRectangle, BorderAndStatus, ButtonBorderStyle.Solid);
    }
}
