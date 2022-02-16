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

        using var mainForm = scope.ServiceProvider.GetRequiredService<IMainForm>();

        try
        {
            Application.Run(mainForm as MainForm);
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
