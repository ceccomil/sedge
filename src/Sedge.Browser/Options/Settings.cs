namespace Sedge.Browser.Options;

public class Settings
{
    [JsonIgnore]
    public string WindowSettingsFile { get; set; }
    public bool IsMaximized { get; set; }
    public int X { get; set; } = 100;
    public int Y { get; set; } = 100;
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 600;

    private Settings(string fileName)
    {
        WindowSettingsFile = fileName;
    }

    public static async Task<Settings> ReadSettings(string file)
    {
        var winSettings = new Settings(file);

        if (!File.Exists(file))
            return winSettings;

        var json = await File.ReadAllBytesAsync(file);
        var parsed = JsonDocument.Parse(json);

        winSettings.IsMaximized = parsed
            .RootElement
            .GetProperty(nameof(winSettings.IsMaximized))
            .GetBoolean();

        winSettings.X = parsed
            .RootElement
            .GetProperty(nameof(winSettings.X))
            .GetInt32();

        winSettings.Y = parsed
            .RootElement
            .GetProperty(nameof(winSettings.Y))
            .GetInt32();

        winSettings.Width = parsed
            .RootElement
            .GetProperty(nameof(winSettings.Width))
            .GetInt32();

        winSettings.Height = parsed
            .RootElement
            .GetProperty(nameof(winSettings.Height))
            .GetInt32();

        return winSettings;
    }

    public static async Task SaveSettings(Settings settings)
    {
        if (File.Exists(settings.WindowSettingsFile))
            File.Delete(settings.WindowSettingsFile);

        var json = JsonSerializer.SerializeToUtf8Bytes(settings);

        await File.WriteAllBytesAsync(settings.WindowSettingsFile, json);
    }
}
