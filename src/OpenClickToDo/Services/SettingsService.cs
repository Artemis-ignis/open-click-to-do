using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenClickToDo.Models;

namespace OpenClickToDo.Services;

public sealed class AppSettings
{
    public TranslationProvider TranslationProvider { get; set; } = TranslationProvider.Browser;

    public ActionCommand DefaultAction { get; set; } = ActionCommand.Copy;

    public string BrowserTranslateUrlTemplate { get; set; } =
        "https://translate.google.com/?sl=auto&tl=auto&text={0}&op=translate";

    public string SearchUrlTemplate { get; set; } = "https://www.google.com/search?q={0}";

    public string OllamaEndpoint { get; set; } = "http://localhost:11434";

    public string OllamaModel { get; set; } = "llama3.1";

    public string TargetLanguage { get; set; } = "auto";
}

public sealed class SettingsService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public SettingsService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public string SettingsDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OpenClickToDo");

    public string SettingsFilePath => Path.Combine(SettingsDirectory, "settings.json");

    public AppSettings Load()
    {
        Directory.CreateDirectory(SettingsDirectory);
        if (!File.Exists(SettingsFilePath))
        {
            var created = new AppSettings();
            Save(created);
            return created;
        }

        try
        {
            var text = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(text, _jsonOptions) ?? new AppSettings();
        }
        catch
        {
            var backupPath = SettingsFilePath + ".broken";
            File.Copy(SettingsFilePath, backupPath, overwrite: true);
            var fallback = new AppSettings();
            Save(fallback);
            return fallback;
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsDirectory);
        File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings, _jsonOptions));
    }
}

