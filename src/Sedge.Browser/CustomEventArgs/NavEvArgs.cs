namespace Sedge.Browser.CustomEventArgs;

public class NavEvArgs : EventArgs
{
    public Uri Url { get; }
    public bool NewWindow { get; }
    public NavEvArgs(
        Uri url,
        bool newWindow = false)
    {
        Url = url;
        NewWindow = newWindow;
    }
}
