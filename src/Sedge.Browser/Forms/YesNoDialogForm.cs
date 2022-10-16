using Sedge.Browser.Controls;
using System.Drawing;

namespace Sedge.Browser.Forms;

public interface IYesNoDialogForm
{
    DialogResult ShowDialog(
        IWin32Window owner,
        string question);
}

[DesignerCategory("Code")]
public class YesNoDialogForm : Form, IYesNoDialogForm
{
    private readonly ICaptainLogger _logger;
    private readonly Color _borderColor = BorderAndStatusChildren;
    private readonly Color _backColor = DarkPanelChildren;
    private readonly IDrawBorders _drawBorders;
    private readonly BoxButton _boxClose = new(BoxButtons.Close);
    private readonly Label _question = new();
    private readonly Button _yes = new();
    private readonly Button _no = new();

    public YesNoDialogForm(
        ICaptainLogger<YesNoDialogForm> logger,
        IDrawBorders drawBorders)
    {
        _logger = logger;
        _drawBorders = drawBorders;
        Init();
    }

    private void Init()
    {
        SuspendLayout();
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new(680, 320);
        Text = "Sedge (Yes/No)";
        BackColor = DarkPanel;
        TopMost = true;
        DialogResult = DialogResult.Cancel;
        ShowInTaskbar = false;
        ShowIcon = false;
        ControlBox = false;

        _boxClose.Size = new(36, 32);
        _boxClose.Location = new(Right - 37, 1);
        _boxClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _boxClose.Click += (o, e) => Close();
        _boxClose.BackColor = _backColor;

        _question.AutoSize = true;
        _question.Top = 60;
        _question.Left = 10;
        _question.Height = 100;
        _question.Width = 660;
        _question.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        _question.ForeColor = GrayWhite;
        _question.Font = new("Cascadia Code", 11.0f);

        SetUpButton(_yes, "Yes");
        SetUpButton(_no, "No");

        _no.Location = new(590, 265);
        _yes.Location = new(510, 265);

        Controls.Add(_boxClose);
        Controls.Add(_question);
        Controls.Add(_yes);
        Controls.Add(_no);

        ResumeLayout(false);
    }

    private void SetUpButton(
        Button btn,
        string text)
    {
        btn.Text = text;

        btn.FlatStyle = FlatStyle.Flat;
        btn.ForeColor = GrayWhite;
        btn.BackColor = DarkPanel;

        btn.Font = _question.Font;
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseDownBackColor = BlueBar;
        btn.FlatAppearance.MouseOverBackColor = LightDark;

        btn.Size = new(80, 33);
        btn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        btn.Click += (o, e) =>
        {
            if (o == _yes)
            {
                DialogResult = DialogResult.Yes;
            }

            if (o == _no)
            {
                DialogResult = DialogResult.No;
            }

            Close();
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        _drawBorders
            .DrawRoundCornerAndBorder(
                this,
                e.Graphics,
                _borderColor);
    }

    public DialogResult ShowDialog(
        IWin32Window owner,
        string question)
    {
        _question.Text = question;
        return ShowDialog(owner);
    }
}
