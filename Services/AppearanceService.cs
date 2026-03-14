namespace  LLMThinkTank.Services;

public enum AppearanceMode
{
    Dark,
    Light,
    Spring,
    Summer,
    Autumn,
    Winter,
    Matrix,
    Ice,
    Sunset,
    Neon,
    Dracula,
    Solarized,
    Midnight
}

public class AppearanceService
{
    private readonly LlmThinkTankSettingsService _settings;

    public AppearanceMode Mode { get; private set; } = AppearanceMode.Dark;

    public AppearanceService(LlmThinkTankSettingsService settings)
    {
        _settings = settings;
        Mode = ParseMode(_settings.AppearanceTheme) ?? AppearanceMode.Dark;
    }

    public event Action? Changed;

    public void SetMode(AppearanceMode mode)
    {
        if (Mode == mode)
            return;

        Mode = mode;
        _settings.SetAppearanceTheme(ToThemeValue(mode));
        Changed?.Invoke();
    }

    private static AppearanceMode? ParseMode(string? theme)
        => (theme ?? "").Trim().ToLowerInvariant() switch
        {
            "dark" => AppearanceMode.Dark,
            "light" => AppearanceMode.Light,
            "spring" => AppearanceMode.Spring,
            "summer" => AppearanceMode.Summer,
            "autumn" => AppearanceMode.Autumn,
            "winter" => AppearanceMode.Winter,
            "matrix" => AppearanceMode.Matrix,
            "ice" => AppearanceMode.Ice,
            "sunset" => AppearanceMode.Sunset,
            "neon" => AppearanceMode.Neon,
            "dracula" => AppearanceMode.Dracula,
            "solarized" => AppearanceMode.Solarized,
            "midnight" => AppearanceMode.Midnight,
            _ => null
        };

    public static string ToThemeValue(AppearanceMode mode)
        => mode.ToString().ToLowerInvariant();
}
