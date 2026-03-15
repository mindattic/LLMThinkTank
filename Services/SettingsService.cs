namespace  LLMThinkTank.Services;

public class SettingsService : LlmThinkTankSettingsService
{
}

public class LlmThinkTankSettingsService
{
    public Dictionary<string, ProviderAuthConfig> ProviderAuth { get; } = new();

    public List<ParticipantTemplate> Templates { get; } = new();

    public List<PersistedConversation> Conversations { get; } = new();

    private const string SettingsFileName = "Settings.json";

    public string? AppearanceTheme { get; private set; } = "dark";

    public LlmThinkTankSettingsService()
    {
        LoadOrInit();
    }

    private static string SettingsRoot
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MindAttic", "LLMThinkTank");

    private static string PersonalitiesRoot
        => Path.Combine(SettingsRoot, "Personalities");

    private static string SettingsPath
        => Path.Combine(SettingsRoot, SettingsFileName);

    private void LoadOrInit()
    {
        if (TryLoad())
        {
            EnsureDefaultsIfMissing();
            EnsurePersonalityFiles();
            return;
        }

        ProviderAuth["openai"] = new ProviderAuthConfig("openai", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"gpt-4\"\n}");
        ProviderAuth["deepseek"] = new ProviderAuthConfig("deepseek", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"deepseek-chat\"\n}");
        ProviderAuth["claude"] = new ProviderAuthConfig("claude", "{\n  \"type\": \"anthropic\",\n  \"apiKey\": \"\",\n  \"version\": \"2023-06-01\",\n  \"model\": \"claude-sonnet-4\"\n}");
        ProviderAuth["gemini"] = new ProviderAuthConfig("gemini", "{\n  \"type\": \"google\",\n  \"apiKey\": \"\",\n  \"model\": \"gemini-2.5-flash\"\n}");

        Templates.Add(new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "openai",
            DisplayName: "ChatGPT",
            PersonalityMarkdown: "You are ChatGPT, made by OpenAI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be conversational and curious. 2-3 sentences max.",
            AuthOverrideJson: null));

        Templates.Add(new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "claude",
            DisplayName: "Claude",
            PersonalityMarkdown: "You are Claude, made by Anthropic. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be thoughtful and honest. 2-3 sentences max.",
            AuthOverrideJson: null));

        Templates.Add(new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "gemini",
            DisplayName: "Gemini",
            PersonalityMarkdown: "You are Gemini, made by Google. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be analytical and creative. 2-3 sentences max.",
            AuthOverrideJson: null));

        Templates.Add(new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "deepseek",
            DisplayName: "DeepSeek",
            PersonalityMarkdown: "You are DeepSeek, made by DeepSeek AI. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be precise and insightful. 2-3 sentences max.",
            AuthOverrideJson: null));

        AppearanceTheme = "dark";
        EnsurePersonalityFiles();
        Save();
    }

    private void EnsureDefaultsIfMissing()
    {
        try
        {
            if (Templates.Count > 0)
                return;

            Templates.Add(new ParticipantTemplate(
                TemplateId: ChatConversationsService.NewId(),
                ProviderId: "openai",
                DisplayName: "ChatGPT",
                PersonalityMarkdown: "You are ChatGPT, made by OpenAI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be conversational and curious. 2-3 sentences max.",
                AuthOverrideJson: null));

            Templates.Add(new ParticipantTemplate(
                TemplateId: ChatConversationsService.NewId(),
                ProviderId: "claude",
                DisplayName: "Claude",
                PersonalityMarkdown: "You are Claude, made by Anthropic. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be thoughtful and honest. 2-3 sentences max.",
                AuthOverrideJson: null));

            Templates.Add(new ParticipantTemplate(
                TemplateId: ChatConversationsService.NewId(),
                ProviderId: "gemini",
                DisplayName: "Gemini",
                PersonalityMarkdown: "You are Gemini, made by Google. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be analytical and creative. 2-3 sentences max.",
                AuthOverrideJson: null));

            Templates.Add(new ParticipantTemplate(
                TemplateId: ChatConversationsService.NewId(),
                ProviderId: "deepseek",
                DisplayName: "DeepSeek",
                PersonalityMarkdown: "You are DeepSeek, made by DeepSeek AI. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be precise and insightful. 2-3 sentences max.",
                AuthOverrideJson: null));

            Save();
        }
        catch { }
    }

    private void EnsurePersonalityFiles()
    {
        try
        {
            Directory.CreateDirectory(PersonalitiesRoot);

            WriteIfMissing("OpenAI.md",
                "# OpenAI\n\nYou are ChatGPT, made by OpenAI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be conversational and curious. 2-3 sentences max.\n");

            WriteIfMissing("Claude.md",
                "# Claude\n\nYou are Claude, made by Anthropic. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be thoughtful and honest. 2-3 sentences max.\n");

            WriteIfMissing("Gemini.md",
                "# Gemini\n\nYou are Gemini, made by Google. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be analytical and creative. 2-3 sentences max.\n");

            WriteIfMissing("DeepSeek.md",
                "# DeepSeek\n\nYou are DeepSeek, made by DeepSeek AI. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be precise and insightful. 2-3 sentences max.\n");
        }
        catch { }

        void WriteIfMissing(string fileName, string markdown)
        {
            var path = Path.Combine(PersonalitiesRoot, fileName);
            if (!File.Exists(path))
                File.WriteAllText(path, markdown);
        }
    }

    private bool TryLoad()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return false;

            var json = File.ReadAllText(SettingsPath);
            var dto = System.Text.Json.JsonSerializer.Deserialize<PersistedSettings>(json);
            if (dto is null)
                return false;

            ProviderAuth.Clear();
            foreach (var kvp in dto.ProviderAuth)
                ProviderAuth[kvp.Key] = new ProviderAuthConfig(kvp.Key, kvp.Value ?? "{}");

            Templates.Clear();
            if (dto.Templates is not null)
                Templates.AddRange(dto.Templates);

            Conversations.Clear();
            if (dto.Conversations is not null)
                Conversations.AddRange(dto.Conversations);

            AppearanceTheme = string.IsNullOrWhiteSpace(dto.AppearanceTheme) ? "dark" : dto.AppearanceTheme;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void Save()
    {
        try
        {
            var dto = new PersistedSettings
            {
                ProviderAuth = ProviderAuth.ToDictionary(k => k.Key, v => (string?)v.Value.Json),
                Templates = Templates,
                Conversations = Conversations,
                AppearanceTheme = AppearanceTheme
            };

            var json = System.Text.Json.JsonSerializer.Serialize(dto, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            Directory.CreateDirectory(SettingsRoot);
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }

    public string GetAuthJson(string providerId)
        => ProviderAuth.TryGetValue(providerId, out var cfg) ? cfg.Json : "{}";

    public void SetAuthJson(string providerId, string json)
    {
        ProviderAuth[providerId] = new ProviderAuthConfig(providerId, json);
        Save();
    }

    public void SetAppearanceTheme(string theme)
    {
        AppearanceTheme = string.IsNullOrWhiteSpace(theme) ? "dark" : theme;
        Save();
    }

    public void SetConversations(IEnumerable<PersistedConversation> convos)
    {
        Conversations.Clear();
        Conversations.AddRange(convos);
        Save();
    }

    public void SaveTemplates(IEnumerable<ParticipantTemplate> templates)
    {
        Templates.Clear();
        Templates.AddRange(templates);
        Save();
    }

    public string GetKeyForProvider(string providerId, string? apiKeyOverride)
    {
        if (!string.IsNullOrWhiteSpace(apiKeyOverride))
            return apiKeyOverride;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(GetAuthJson(providerId));
            if (doc.RootElement.TryGetProperty("apiKey", out var apiKey))
                return apiKey.GetString() ?? "";
        }
        catch { }

        return "";
    }

    public void SetKey(string providerId, string apiKey)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(GetAuthJson(providerId));
            var type = doc.RootElement.TryGetProperty("type", out var t) ? t.GetString() : null;
            var model = doc.RootElement.TryGetProperty("model", out var m) ? m.GetString() : null;
            var extra = GetAuthJson(providerId);
            if (string.IsNullOrWhiteSpace(type))
                type = providerId is "openai" or "deepseek" ? "bearer" : providerId;

            if (string.IsNullOrWhiteSpace(model))
                SetAuthJson(providerId, $"{{\n  \"type\": \"{type}\",\n  \"apiKey\": \"{apiKey}\"\n}}");
            else
                SetAuthJson(providerId, $"{{\n  \"type\": \"{type}\",\n  \"apiKey\": \"{apiKey}\",\n  \"model\": \"{model}\"\n}}");
        }
        catch
        {
            SetAuthJson(providerId, $"{{\n  \"type\": \"{providerId}\",\n  \"apiKey\": \"{apiKey}\"\n}}");
        }
    }

    private sealed class PersistedSettings
    {
        public Dictionary<string, string?> ProviderAuth { get; set; } = new();
        public List<ParticipantTemplate>? Templates { get; set; }
        public List<PersistedConversation>? Conversations { get; set; }
        public string? AppearanceTheme { get; set; }
    }

    public sealed record PersistedConversation(
        string ChatId,
        string Title,
        List<PersistedParticipant> Participants,
        string? Topic,
        List<PersistedMessage>? Messages)
    {
        public List<PersistedStatusEvent>? StatusEvents { get; init; }
        public List<PersistedStatusEvent>? Diagnostics { get; init; }
    }

    public sealed record PersistedMessage(
        string ParticipantId,
        string Text,
        int Round,
        bool IsError);

    public sealed record PersistedStatusEvent(
        DateTimeOffset Timestamp,
        string Text);

    public sealed record PersistedParticipant(
        string ParticipantId,
        string TemplateId,
        string ProviderId,
        string DisplayName,
        string PersonalityMarkdown,
        string? AuthOverrideJson);
}
