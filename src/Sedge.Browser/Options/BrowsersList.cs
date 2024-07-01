namespace Sedge.Browser.Options;

public interface IBrowsersList
{
    List<string> Urls { get; }
    void Init(string[] args);
}

internal sealed class BrowsersList(
    ICaptainLogger<BrowsersList> _logger) : IBrowsersList
{
    public List<string> Urls { get; } = [];

    public void Init(string[] args)
    {
        _logger.InformationLog(
            $"Arguments: {string.Join(',', args)}");

        if (args.Length <= 2)
        {
            return;
        }

        for (var i = 1; i < args.Length - 1; i++)
        {
            Urls.Add(args[i]);
        }
    }
}
