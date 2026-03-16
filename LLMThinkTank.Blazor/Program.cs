using LLMThinkTank.Core.Services;
using LLMThinkTank.Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<SettingsService>();
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
