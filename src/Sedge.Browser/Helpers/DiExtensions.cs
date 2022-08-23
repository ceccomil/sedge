namespace Sedge.Browser.Helpers;

internal static class DiExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, string[]? args = default)
    {
        if (args is null)
            args = Array.Empty<string>();

        var (startUrl, userData, isShared) = args.GetUrlAndUserData();

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services
            .AddSingleton<IConfiguration>(config)
            .Configure<CaptainLoggerOptions>(opts =>
            {
                opts.TimeIsUtc = true;
                opts.LogRecipients = Recipients.File;
                opts.FileRotation = LogRotation.Hour;
                opts.FilePath = Path.Combine(LocalPath, "Logs", "Sedge-Browser.log");

#if DEBUG
                opts.LogRecipients |= Recipients.Console;
#endif
            })
            .AddLogging(builder =>
            {
                builder
                    .ClearProviders()
                    .AddCaptainLogger()
                    .AddFilter("System", LogLevel.Trace)
                    .AddFilter("Microsoft", LogLevel.Trace)
                    .AddFilter(typeof(Program).Namespace, LogLevel.Information);
            })
            .Configure<SedgeBrowserOptions>(async opts =>
            {
                opts.StartUrl = startUrl;
                opts.UserData = userData;
                opts.IsShared = isShared;

                opts
                    .WindowSettings = await Settings
                    .ReadSettings(Path.Combine(LocalPath,
                    $"SettingsFor{userData}.json"));
            })
            .AddSingleton<IBrowserFormCollection, BrowserFormCollection>()
            .AddSingleton<IDrawBorders, DrawBorders>()
            .AddSingleton<IProcessHooks, ProcessHooks>()
            .AddSingleton<IBrowserEnv, BrowserEnv>()
            .AddSingleton<IYesNoDialogForm, YesNoDialogForm>();

        return services;
    }
}
