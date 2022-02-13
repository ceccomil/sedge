namespace Sedge.Browser.Controls;

[DesignerCategory("Code")]
public class FlatButton : Button
{
    protected override bool ShowFocusCues => false;

    public FlatButton() : base() { }
    ~FlatButton() => Dispose(false);

    public override void NotifyDefault(bool value) => base.NotifyDefault(false);
}
