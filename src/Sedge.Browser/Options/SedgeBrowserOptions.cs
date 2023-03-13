namespace Sedge.Browser.Options;

public class SedgeBrowserOptions
{
    public string[] StartPages { get; set; } = null!;
    public string UserData { get; set; } = null!;
    public string UserDataPath => Path.Combine(LocalPath, UserData);
    public bool IsShared { get; set; }
    public Settings WindowSettings { get; set; } = null!;
}
