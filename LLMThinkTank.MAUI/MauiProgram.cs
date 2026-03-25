using Microsoft.Extensions.Logging;
using LLMThinkTank.Core.Services;

namespace LLMThinkTank.MAUI
{
    /// <summary>
    /// Application entry point and dependency injection composition root.
    /// Registers all core services as singletons to ensure shared state across the
    /// Blazor WebView components that make up the LLM Think Tank UI.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Configures fonts, Blazor WebView, and registers all application services:
        /// <list type="bullet">
        ///   <item><see cref="SettingsService"/> / <see cref="LlmThinkTankSettingsService"/> - Persistent configuration</item>
        ///   <item><see cref="LlmThinkTankService"/> - Multi-provider LLM API gateway</item>
        ///   <item><see cref="ChatConversationsService"/> - Conversation tab lifecycle</item>
        ///   <item><see cref="AppearanceService"/> - Theme and layout management</item>
        ///   <item><see cref="ChatLogService"/> - Diagnostic event logging</item>
        ///   <item><see cref="HumanNameService"/> / <see cref="NameGeneratorService"/> - Participant naming</item>
        /// </list>
        /// </summary>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<LlmThinkTankSettingsService>(sp => sp.GetRequiredService<SettingsService>());
            builder.Services.AddSingleton<ChatLogService>();
            builder.Services.AddSingleton<AppearanceService>();
            builder.Services.AddSingleton<ChatConversationsService>();
            builder.Services.AddSingleton<HumanNameService>();
            builder.Services.AddSingleton<NameGeneratorService>();
            builder.Services.AddSingleton<LlmThinkTankService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
