namespace Sedge.Browser.Controls;

public interface IUrlNavigation
{
    event EventHandler<NavEvArgs>? Navigate;
    string Url { get; set; }
    Point Location { get; set; }
    bool IsVisible { get; }
    bool HideBtnOnClose { get; set; }
    void ToggleShow();
    void GoToUrlNewWindow(string url);
}
