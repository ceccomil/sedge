namespace Sedge.Browser.Forms;

public interface IBrowserFormCollection : ICollection<BrowserForm>
{
    SearchEngines SearchEngine { get; }
    IBrowserForm MainForm { get; }
    IBrowserForm AppendNew();
}
