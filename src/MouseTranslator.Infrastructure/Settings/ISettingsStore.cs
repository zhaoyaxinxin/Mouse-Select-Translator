namespace MouseTranslator.Infrastructure.Settings;

public interface ISettingsStore
{
    AppSettings LoadOrCreate();

    void Save(AppSettings settings);

    string SettingsPath { get; }
}
