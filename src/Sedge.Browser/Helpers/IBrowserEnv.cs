namespace Sedge.Browser.Helpers;

public interface IBrowserEnv
{
    CoreWebView2Environment? Environment { get; }

    Task SetupEnvironment();
}
