namespace  LLMThinkTank.Services;

public class NameGeneratorService
{
    private readonly LlmThinkTankService _thinkTank;
    private readonly LlmThinkTankSettingsService _settings;

    public NameGeneratorService(LlmThinkTankService thinkTank, LlmThinkTankSettingsService settings)
    {
        _thinkTank = thinkTank;
        _settings = settings;
    }

    public async Task<string> GenerateFirstNameAsync(string providerId, string? authOverrideJson = null, CancellationToken cancellationToken = default)
    {
        var prompt = "Pick a single realistic first name for a human. Output ONLY the name, no punctuation, no quotes.";

        var personality = "You generate a single realistic human first name. Output only the name.";

        var history = new List<SharedTurn>();

        var name = await _thinkTank.CallProvider(providerId, personality, authOverrideJson, prompt, history);

        name = (name ?? "").Trim();
        if (name.Length > 32)
            name = name[..32];

        name = new string(name.Where(char.IsLetter).ToArray());
        if (string.IsNullOrWhiteSpace(name))
            name = "Alex";

        return name;
    }
}
