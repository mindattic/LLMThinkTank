using LLMThinkTank.Core.Models;

namespace LLMThinkTank.Core.Services;

public class AppearanceService
{
    private readonly LlmThinkTankSettingsService _settings;

    public AppearanceMode Mode { get; private set; } = AppearanceMode.Dark;
    public int ControlHeight { get; private set; } = 40;
    public int Gutter { get; private set; } = 10;
    public int BorderRadius { get; private set; } = 10;

    public AppearanceService(LlmThinkTankSettingsService settings)
    {
        _settings = settings;
        Mode = ParseMode(_settings.AppearanceTheme) ?? AppearanceMode.Dark;
        ControlHeight = _settings.ControlHeight ?? 40;
        Gutter = _settings.Gutter ?? 10;
        BorderRadius = _settings.BorderRadius ?? 10;
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

    public void SetControlHeight(int height)
    {
        height = Math.Clamp(height, 28, 60);
        if (ControlHeight == height)
            return;

        ControlHeight = height;
        _settings.SetControlHeight(height);
        Changed?.Invoke();
    }

    public void SetGutter(int px)
    {
        px = Math.Clamp(px, 0, 30);
        if (Gutter == px) return;
        Gutter = px;
        _settings.SetGutter(px);
        Changed?.Invoke();
    }

    public void SetBorderRadius(int px)
    {
        px = Math.Clamp(px, 0, 24);
        if (BorderRadius == px) return;
        BorderRadius = px;
        _settings.SetBorderRadius(px);
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
