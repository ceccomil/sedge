namespace Sedge.Browser.Forms;

public interface IBrowserFormCollection : ICollection<BrowserForm>
{
    FileInfo? ExternalBrowser { get; }
    SearchEngines SearchEngine { get; }
    IBrowserForm MainForm { get; }
    IBrowserForm AppendNew();

    IYesNoDialogForm YesNoDialogForm { get; }
}
