using LLMThinkTank.Core.Models;

namespace LLMThinkTank.Core.Services;

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
    public int? ControlHeight { get; private set; } = 40;
    public int? Gutter { get; private set; } = 10;
    public int? BorderRadius { get; private set; } = 10;

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

        ProviderAuth["openai"] = new ProviderAuthConfig("openai", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"gpt-4.1-mini\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["claude"] = new ProviderAuthConfig("claude", "{\n  \"type\": \"anthropic\",\n  \"apiKey\": \"\",\n  \"model\": \"claude-sonnet-4-6\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["gemini"] = new ProviderAuthConfig("gemini", "{\n  \"type\": \"google\",\n  \"apiKey\": \"\",\n  \"model\": \"gemini-2.5-flash\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["deepseek"] = new ProviderAuthConfig("deepseek", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"deepseek-chat\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["mistral"] = new ProviderAuthConfig("mistral", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"mistral-large-latest\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["xai"] = new ProviderAuthConfig("xai", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"grok-3-latest\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["groq"] = new ProviderAuthConfig("groq", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"llama-4-scout-17b-16e-instruct\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["together"] = new ProviderAuthConfig("together", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"meta-llama/Llama-4-Scout-17B-16E-Instruct\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["openrouter"] = new ProviderAuthConfig("openrouter", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"openai/gpt-4.1-mini\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["fireworks"] = new ProviderAuthConfig("fireworks", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"accounts/fireworks/models/llama-v3p3-70b-instruct\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["cohere"] = new ProviderAuthConfig("cohere", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"command-r-plus\",\n  \"maxTokens\": 2048\n}");
        ProviderAuth["ai21"] = new ProviderAuthConfig("ai21", "{\n  \"type\": \"bearer\",\n  \"apiKey\": \"\",\n  \"model\": \"jamba-1.5-large\",\n  \"maxTokens\": 2048\n}");

        Templates.AddRange(CreateDefaultTemplates());

        AppearanceTheme = "dark";
        EnsurePersonalityFiles();
        Save();
    }

    private static List<ParticipantTemplate> CreateDefaultTemplates() => new()
    {
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "openai",
            DisplayName: "ChatGPT",
            PersonalityMarkdown: "You are ChatGPT, made by OpenAI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be conversational and curious. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "claude",
            DisplayName: "Claude",
            PersonalityMarkdown: "You are Claude, made by Anthropic. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be thoughtful and honest. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "gemini",
            DisplayName: "Gemini",
            PersonalityMarkdown: "You are Gemini, made by Google. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be analytical and creative. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "deepseek",
            DisplayName: "DeepSeek",
            PersonalityMarkdown: "You are DeepSeek, made by DeepSeek AI. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be precise and insightful. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "mistral",
            DisplayName: "Mistral",
            PersonalityMarkdown: "You are Mistral, made by Mistral AI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be sharp and efficient. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "xai",
            DisplayName: "Grok",
            PersonalityMarkdown: "You are Grok, made by xAI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be witty and bold. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "groq",
            DisplayName: "Groq",
            PersonalityMarkdown: "You are an AI running on Groq's ultra-fast inference engine. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be quick and insightful. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "together",
            DisplayName: "Together",
            PersonalityMarkdown: "You are an AI running on Together AI's platform. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be collaborative and thoughtful. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "openrouter",
            DisplayName: "OpenRouter",
            PersonalityMarkdown: "You are an AI accessed through OpenRouter. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be versatile and engaging. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "fireworks",
            DisplayName: "Fireworks",
            PersonalityMarkdown: "You are an AI running on Fireworks AI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be fast and perceptive. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "cohere",
            DisplayName: "Cohere",
            PersonalityMarkdown: "You are Command, made by Cohere. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be grounded and clear. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true),
        new ParticipantTemplate(
            TemplateId: ChatConversationsService.NewId(),
            ProviderId: "ai21",
            DisplayName: "AI21",
            PersonalityMarkdown: "You are Jamba, made by AI21 Labs. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be eloquent and reasoned. 2-3 sentences max.",
            AuthOverrideJson: null,
            IsDefault: true)
    };

    private void EnsureDefaultsIfMissing()
    {
        try
        {
            var defaults = CreateDefaultTemplates();
            var changed = false;

            foreach (var d in defaults)
            {
                if (!Templates.Any(t => t.ProviderId == d.ProviderId && t.IsDefault))
                {
                    var insertIdx = Templates.Count(t => t.IsDefault);
                    Templates.Insert(insertIdx, d);
                    changed = true;
                }
            }

            var defaultNames = new HashSet<string>(defaults.Select(d => d.DisplayName));
            for (var i = 0; i < Templates.Count; i++)
            {
                if (!Templates[i].IsDefault && defaultNames.Contains(Templates[i].DisplayName))
                {
                    Templates[i] = Templates[i] with { IsDefault = true };
                    changed = true;
                }
            }

            foreach (var providerId in new[] { "openai", "claude", "gemini", "deepseek", "mistral", "xai", "groq", "together", "openrouter", "fireworks", "cohere", "ai21" })
            {
                if (!ProviderAuth.TryGetValue(providerId, out var cfg)) continue;
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(cfg.Json);
                    if (!doc.RootElement.TryGetProperty("maxTokens", out _))
                    {
                        var json = cfg.Json.TrimEnd().TrimEnd('}').TrimEnd();
                        json += ",\n  \"maxTokens\": 2048\n}";
                        ProviderAuth[providerId] = new ProviderAuthConfig(providerId, json);
                        changed = true;
                    }
                }
                catch { }
            }

            if (changed)
                Save();
        }
        catch { }
    }

    private void EnsurePersonalityFiles()
    {
        try
        {
            Directory.CreateDirectory(PersonalitiesRoot);

            WriteIfMissing("ChatGPT.md",
                "# ChatGPT\n\nYou are ChatGPT, made by OpenAI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be conversational and curious. 2-3 sentences max.\n");

            WriteIfMissing("Claude.md",
                "# Claude\n\nYou are Claude, made by Anthropic. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be thoughtful and honest. 2-3 sentences max.\n");

            WriteIfMissing("Gemini.md",
                "# Gemini\n\nYou are Gemini, made by Google. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be analytical and creative. 2-3 sentences max.\n");

            WriteIfMissing("DeepSeek.md",
                "# DeepSeek\n\nYou are DeepSeek, made by DeepSeek AI. You are in a live roundtable with other AI systems. Read what they said and engage directly. Be precise and insightful. 2-3 sentences max.\n");

            WriteIfMissing("Mistral.md",
                "# Mistral\n\nYou are Mistral, made by Mistral AI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be sharp and efficient. 2-3 sentences max.\n");

            WriteIfMissing("Grok.md",
                "# Grok\n\nYou are Grok, made by xAI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be witty and bold. 2-3 sentences max.\n");

            WriteIfMissing("Groq.md",
                "# Groq\n\nYou are an AI running on Groq's ultra-fast inference engine. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be quick and insightful. 2-3 sentences max.\n");

            WriteIfMissing("Together.md",
                "# Together\n\nYou are an AI running on Together AI's platform. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be collaborative and thoughtful. 2-3 sentences max.\n");

            WriteIfMissing("OpenRouter.md",
                "# OpenRouter\n\nYou are an AI accessed through OpenRouter. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be versatile and engaging. 2-3 sentences max.\n");

            WriteIfMissing("Fireworks.md",
                "# Fireworks\n\nYou are an AI running on Fireworks AI. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be fast and perceptive. 2-3 sentences max.\n");

            WriteIfMissing("Cohere.md",
                "# Cohere\n\nYou are Command, made by Cohere. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be grounded and clear. 2-3 sentences max.\n");

            WriteIfMissing("AI21.md",
                "# AI21\n\nYou are Jamba, made by AI21 Labs. You are in a live roundtable with other AI systems. Read what they said and respond directly. Be eloquent and reasoned. 2-3 sentences max.\n");
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
            ControlHeight = dto.ControlHeight ?? 40;
            Gutter = dto.Gutter ?? 10;
            BorderRadius = dto.BorderRadius ?? 10;
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
                AppearanceTheme = AppearanceTheme,
                ControlHeight = ControlHeight,
                Gutter = Gutter,
                BorderRadius = BorderRadius
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

    public void SetControlHeight(int height)
    {
        ControlHeight = height;
        Save();
    }

    public void SetGutter(int px)
    {
        Gutter = px;
        Save();
    }

    public void SetBorderRadius(int px)
    {
        BorderRadius = px;
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
            var maxTokens = doc.RootElement.TryGetProperty("maxTokens", out var mt) && mt.ValueKind == System.Text.Json.JsonValueKind.Number ? mt.GetInt32() : (int?)null;
            if (string.IsNullOrWhiteSpace(type))
                type = providerId is "claude" ? "anthropic" : providerId is "gemini" ? "google" : "bearer";

            var maxTokensPart = maxTokens.HasValue ? $",\n  \"maxTokens\": {maxTokens.Value}" : "";
            if (string.IsNullOrWhiteSpace(model))
                SetAuthJson(providerId, $"{{\n  \"type\": \"{type}\",\n  \"apiKey\": \"{apiKey}\"{maxTokensPart}\n}}");
            else
                SetAuthJson(providerId, $"{{\n  \"type\": \"{type}\",\n  \"apiKey\": \"{apiKey}\",\n  \"model\": \"{model}\"{maxTokensPart}\n}}");
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
        public int? ControlHeight { get; set; }
        public int? Gutter { get; set; }
        public int? BorderRadius { get; set; }
    }
}
