namespace Sedge.Browser.Helpers;

public class BrowserEnv : IBrowserEnv
{
    public CoreWebView2Environment? Environment { get; private set; }

    private readonly ICaptainLogger _logger;
    private readonly SedgeBrowserOptions _options;

    public BrowserEnv(
        ICaptainLogger<BrowserEnv> logger,
        IOptions<SedgeBrowserOptions> opts)
    {
        _logger = logger;
        _options = opts.Value;
    }

    public async Task SetupEnvironment()
    {
        if (Environment is not null)
            return;

        Environment = await CoreWebView2Environment
            .CreateAsync(
                null,
                _options.UserDataPath);

        _logger
            .InformationLog(
                $"Environment has been setup, user data path: `{_options.UserDataPath}`");
    }
}
