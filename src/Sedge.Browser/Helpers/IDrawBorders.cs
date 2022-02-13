namespace Sedge.Browser.Helpers;

public interface IDrawBorders
{
    int OsMajorVersion { get; }
    bool IsGpuCapable { get; }
    void DrawRoundCornerAndBorder(Form form, Graphics g, Color color);
}
