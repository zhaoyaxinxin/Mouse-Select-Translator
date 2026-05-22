namespace MouseTranslator.Infrastructure.Settings;

public sealed class AppSettings
{
    public GeneralSettings General { get; init; } = new();

    public SelectionSettings Selection { get; init; } = new();

    public TranslationSettings Translation { get; init; } = new();

    public PrivacySettings Privacy { get; init; } = new();

    public OverlaySettings Overlay { get; init; } = new();

    public List<string> AppBlacklist { get; init; } = new()
    {
        "KeePass.exe",
        "1Password.exe",
        "Bitwarden.exe",
    };

    public static AppSettings CreateDefault() => new();
}

public sealed class GeneralSettings
{
    public bool EnabledOnStartup { get; init; } = true;

    public bool StartMinimized { get; init; } = true;

    public string HotkeyToggle { get; init; } = "Ctrl+Shift+T";
}

public sealed class SelectionSettings
{
    public int MinDragDistancePx { get; init; } = 8;

    public int DelayAfterMouseUpMs { get; init; } = 150;

    public int MinTextLength { get; init; } = 2;

    public int MaxTextLength { get; init; } = 2000;

    public int DuplicateIgnoreSeconds { get; init; } = 3;
}

public sealed class TranslationSettings
{
    public string Provider { get; init; } = "DeepSeek";

    public string TargetLanguage { get; init; } = "zh-CN";

    public int TimeoutSeconds { get; init; } = 15;

    public string BaseUrl { get; init; } = "https://api.deepseek.com";

    public string Model { get; init; } = "deepseek-v4-flash";

    public string ApiKeyEnvironmentVariable { get; init; } = "DEEPSEEK_API_KEY";
}

public sealed class PrivacySettings
{
    public bool SaveHistory { get; init; }

    public bool LogSelectedText { get; init; }

    public bool SkipPasswordFields { get; init; } = true;
}

public sealed class OverlaySettings
{
    public double MaxWidth { get; init; } = 420;

    public double AutoHideSeconds { get; init; } = 10;

    public double OffsetY { get; init; } = 20;

    public double DismissDistancePixels { get; init; } = 64;
}
