using System.IO;
using System.Text.Json;

namespace MouseTranslator.Infrastructure.Settings;

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
    };

    public JsonSettingsStore()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var directory = Path.Combine(appDataPath, "MouseSelectTranslator");
        Directory.CreateDirectory(directory);
        SettingsPath = Path.Combine(directory, "settings.json");
    }

    public string SettingsPath { get; }

    public AppSettings LoadOrCreate()
    {
        if (!File.Exists(SettingsPath))
        {
            var defaults = AppSettings.CreateDefault();
            Save(defaults);
            return defaults;
        }

        var json = File.ReadAllText(SettingsPath);
        var settings = JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions);
        if (settings is null)
        {
            settings = AppSettings.CreateDefault();
            Save(settings);
            return settings;
        }

        var migrated = Migrate(settings);
        if (!ReferenceEquals(migrated, settings))
        {
            settings = migrated;
            Save(settings);
        }

        return settings;
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, SerializerOptions));
    }

    private static AppSettings Migrate(AppSettings settings)
    {
        var translation = settings.Translation;
        var shouldMigrateMockDefaults =
            string.Equals(translation.Provider, "Mock", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(translation.BaseUrl)
            && string.IsNullOrWhiteSpace(translation.Model)
            && string.Equals(translation.ApiKeyEnvironmentVariable, "OPENAI_API_KEY", StringComparison.Ordinal);

        if (!shouldMigrateMockDefaults)
        {
            return settings;
        }

        return new AppSettings
        {
            General = settings.General,
            Selection = settings.Selection,
            Privacy = settings.Privacy,
            Overlay = settings.Overlay,
            AppBlacklist = settings.AppBlacklist,
            Translation = new TranslationSettings
            {
                Provider = "DeepSeek",
                TargetLanguage = translation.TargetLanguage,
                TimeoutSeconds = translation.TimeoutSeconds,
                BaseUrl = "https://api.deepseek.com",
                Model = "deepseek-v4-flash",
                ApiKeyEnvironmentVariable = "DEEPSEEK_API_KEY",
            },
        };
    }
}
