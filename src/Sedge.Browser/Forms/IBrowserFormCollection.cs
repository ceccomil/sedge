namespace Sedge.Browser.Forms;

public interface IBrowserFormCollection : ICollection<BrowserForm>
{
    IBrowserForm MainForm { get; }
    IBrowserForm AppendNew();
}
