namespace Sedge.Browser.Options;

public class SedgeBrowserOptions
{
    public Uri StartUrl { get; set; } = null!;
    public string UserData { get; set; } = null!;
    public string UserDataPath => Path.Combine(LocalPath, UserData);
    public bool IsShared { get; set; }
    public Settings WindowSettings { get; set; } = null!;
}
