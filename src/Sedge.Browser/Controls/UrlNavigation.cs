namespace Sedge.Browser.Controls;

[DesignerCategory("Code")]
public class UrlNavigation : UserControl
{
    private readonly TextBox _url = new();
    private readonly Button _navigate = new();
    private readonly IContainer _components = null!;

    public event EventHandler<NavEvArgs>? Navigate;

    public string Url
    {
        get => _url.Text;
        set => _url.Text = value;
    }

    public UrlNavigation()
    {
        _components = new Container();
        InitializeComponent();

        _navigate.Click += (o, e) =>
        {
            if (Uri.TryCreate(_url.Text, UriKind.Absolute, out Uri? uri))
            {
                Navigate?.Invoke(this, new(uri));
                Visible = false;
            }
        };
    }

    ~UrlNavigation() => Dispose(false);

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

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(DarkControl);
        ControlPaint.DrawBorder(e.Graphics, ClientRectangle, BorderAndStatus, ButtonBorderStyle.Solid);
    }

    public class NavEvArgs : EventArgs
    {
        public Uri Url { get; }
        public NavEvArgs(Uri url)
        {
            Url = url;
        }
    }
}
