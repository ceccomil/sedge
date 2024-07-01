namespace Sedge.Browser;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
        _ = AllocConsole();
#endif

        var services = new ServiceCollection()
            .ConfigureServices(args);

        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var bList = scope
            .ServiceProvider
            .GetRequiredService<IBrowsersList>();

        bList.Init(args);

        var formCollection = scope
            .ServiceProvider
            .GetRequiredService<IBrowserFormCollection>();

        try
        {
            var form = formCollection.AppendNew() as Form
                ?? throw new InvalidOperationException(
                    "Failed to create a new form!");

            Application.Run(form);
        }
        catch (Exception ex)
        {
            LogErrorAndExit(ex, scope.ServiceProvider);
        }

        Exit(scope.ServiceProvider);
    }

    private static void Exit(IServiceProvider sp)
    {
#if DEBUG
        _ = FreeConsole();
#endif

        var hooks = sp.GetRequiredService<IProcessHooks>();
        hooks.Dispose();

        Application.Exit();
    }

    private static void LogErrorAndExit(Exception ex, IServiceProvider sp)
    {
        var logger = sp.GetRequiredService<ICaptainLogger<Program>>();

        logger.ErrorLog($"Unhandled error!\r\n{ex}");
        MessageBox.Show("See the log for detailed error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Exit(sp);
    }
}
