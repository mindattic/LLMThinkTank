using LLMThinkTank.Core.Models;
using LLMThinkTank.Core.Services;
using LLMThinkTank.Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton(sp =>
{
    var settings = new SettingsService();

    var config = sp.GetRequiredService<IConfiguration>();
    var section = config.GetSection("ProviderDefaults");
    foreach (var provider in section.GetChildren())
    {
        var providerId = provider.Key;
        var apiKey = provider["apiKey"] ?? "";
        var model = provider["model"] ?? "";
        var type = providerId is "claude" ? "anthropic" : providerId is "gemini" ? "google" : "bearer";
        var json = string.IsNullOrWhiteSpace(model)
            ? $"{{\n  \"type\": \"{type}\",\n  \"apiKey\": \"{apiKey}\",\n  \"maxTokens\": 2048\n}}"
            : $"{{\n  \"type\": \"{type}\",\n  \"apiKey\": \"{apiKey}\",\n  \"model\": \"{model}\",\n  \"maxTokens\": 2048\n}}";
        settings.ProviderDefaults[providerId] = new ProviderAuthConfig(providerId, json);
    }

    return settings;
});
builder.Services.AddSingleton<LlmThinkTankSettingsService>(sp => sp.GetRequiredService<SettingsService>());
builder.Services.AddSingleton<ChatLogService>();
builder.Services.AddSingleton<AppearanceService>();
builder.Services.AddSingleton<ChatConversationsService>();
builder.Services.AddSingleton<HumanNameService>();
builder.Services.AddSingleton<NameGeneratorService>();
builder.Services.AddSingleton<LlmThinkTankService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(LLMThinkTank.Shared.Components.Pages.Home).Assembly);

app.Run();
