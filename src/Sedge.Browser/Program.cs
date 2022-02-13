namespace Sedge.Browser;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var services = new ServiceCollection()
            .ConfigureServices(args);

        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        using var mainForm = scope.ServiceProvider.GetRequiredService<IMainForm>();
        Application.Run(mainForm as MainForm);
    }
}
