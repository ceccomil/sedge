namespace Sedge.Browser.Helpers;

public static class Globals
{
    public static string LocalPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SedgeData");
}

public enum BoxButtons { Close, Icon, Maximize, Maximized };

public enum SearchEngines { Google = 0, Bing = 1, Yahoo = 2 }
