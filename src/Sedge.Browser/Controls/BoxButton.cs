namespace Sedge.Browser.Controls;

[DesignerCategory("Code")]
public class BoxButton : Button
{
    private readonly string _icon = string.Empty;
    private readonly Font _font = new("Marlett", 12.0f);

    protected override bool ShowFocusCues => false;

    //□ ━ × 
    public BoxButton(BoxButtons boxButton) : base()
    {
        if (boxButton == BoxButtons.Close)
            _icon = "r";

        if (boxButton == BoxButtons.Maximize)
            _icon = "1";

        if (boxButton == BoxButtons.Maximized)
            _icon = "2";

        if (boxButton == BoxButtons.Icon)
            _icon = "0";

        FlatStyle = FlatStyle.Flat;
        Text = _icon;
        ForeColor = GrayWhite;
        BackColor = DarkPanel;

        Font = _font;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseDownBackColor = BlueBar;
        FlatAppearance.MouseOverBackColor = LightDark;

        if (boxButton == BoxButtons.Close)
        {
            FlatAppearance.MouseDownBackColor = Danger;
            FlatAppearance.MouseOverBackColor = Danger;
        }

        Size = new(43, 33);
        Anchor = AnchorStyles.Top | AnchorStyles.Right;
    }

    public void SetMaximized() => Text = "2";
    public void SetMaximize() => Text = "1";

    public override void NotifyDefault(bool value) => base.NotifyDefault(false);

    protected override void Dispose(bool disposing) => base.Dispose(disposing);
}