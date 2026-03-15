using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace  LLMThinkTank.Services;

// ── Data models ───────────────────────────────────────────────────────────────

public class LlmModel
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Avatar { get; init; } = "";
    public string Personality { get; init; } = "";
}

public class SharedTurn
{
    public string ModelId { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string Text { get; set; } = "";
    public int Round { get; set; }
}

public class ConversationMessage
{
    public string ModelId { get; set; } = "";
    public string Text { get; set; } = "";
    public int Round { get; set; }
    public bool IsError { get; set; }
}

// ── Service ───────────────────────────────────────────────────────────────────

public class LlmThinkTankService
{
    private readonly HttpClient _http;
    private readonly LlmThinkTankSettingsService _settings;

    public LlmThinkTankService(HttpClient http, LlmThinkTankSettingsService settings)
    {
        _http = http;
        _settings = settings;
    }

    public List<LlmModel> Models { get; } = new()
    {
        new LlmModel
        {
            Id = "openai",
            Name = "ChatGPT",
            Avatar = "⬡",
            Personality = "You are ChatGPT, made by OpenAI. You are in a live roundtable with three other AI systems: Claude (Anthropic), Gemini (Google), and DeepSeek. Read what they said and respond directly to them and the topic. Be conversational and curious. 2-3 sentences max."
        },
        new LlmModel
        {
            Id = "claude",
            Name = "Claude",
            Avatar = "◈",
            Personality = "You are Claude, made by Anthropic. You are in a live roundtable with three other AI systems: ChatGPT (OpenAI), Gemini (Google), and DeepSeek. Read what they said and engage directly. Be thoughtful and honest. 2-3 sentences max."
        },
        new LlmModel
        {
            Id = "gemini",
            Name = "Gemini",
            Avatar = "✦",
            Personality = "You are Gemini, made by Google. You are in a live roundtable with three other AI systems: ChatGPT (OpenAI), Claude (Anthropic), and DeepSeek. Read what they said and respond directly. Be analytical and creative. 2-3 sentences max."
        },
        new LlmModel
        {
            Id = "deepseek",
            Name = "DeepSeek",
            Avatar = "◉",
            Personality = "You are DeepSeek, made by DeepSeek AI. You are in a live roundtable with three other AI systems: ChatGPT (OpenAI), Claude (Anthropic), and Gemini (Google). Read what they said and engage directly. Be precise and insightful. 2-3 sentences max."
        }
    };

    // ── Main dispatch ────────────────────────────────────────────────────────

    public Task<string> CallModel(LlmModel model, string topic, List<SharedTurn> history)
        => CallProvider(model.Id, model.Personality, authOverrideJson: null, topic, history);

    public Task<string> CallProvider(string providerId, string personalityMarkdown, string? authOverrideJson, string topic, List<SharedTurn> history)
        => providerId switch
        {
            "openai"   => CallOpenAI(providerId, personalityMarkdown, authOverrideJson, topic, history),
            "claude"   => CallClaude(providerId, personalityMarkdown, authOverrideJson, topic, history),
            "gemini"   => CallGemini(providerId, personalityMarkdown, authOverrideJson, topic, history),
            "deepseek" => CallDeepSeek(providerId, personalityMarkdown, authOverrideJson, topic, history),
            _          => throw new ArgumentException($"Unknown provider: {providerId}")
        };

    public event Action<string, string, bool>? Diagnostics;

    private void EmitDiagnostics(string providerId, string raw, bool isError)
    {
        try { Diagnostics?.Invoke(providerId, RedactResponseText(providerId, raw), isError); }
        catch { }
    }

    private static string RedactResponseText(string providerId, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        try
        {
            using var doc = JsonDocument.Parse(raw);

            object? redacted = providerId switch
            {
                "openai" => RedactOpenAi(doc.RootElement),
                "deepseek" => RedactOpenAi(doc.RootElement),
                "claude" => RedactClaude(doc.RootElement),
                "gemini" => RedactGemini(doc.RootElement),
                _ => null
            };

            if (redacted is null)
                return raw;

            return JsonSerializer.Serialize(redacted, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return raw;
        }
    }

    private static object? RedactOpenAi(JsonElement root)
    {
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array)
            return null;

        var newChoices = new List<object>();
        foreach (var c in choices.EnumerateArray())
        {
            var messageRole = c.TryGetProperty("message", out var msg) && msg.TryGetProperty("role", out var role)
                ? role.GetString()
                : null;

            newChoices.Add(new
            {
                index = c.TryGetProperty("index", out var idx) ? idx.GetInt32() : (int?)null,
                finish_reason = c.TryGetProperty("finish_reason", out var fr) ? fr.GetString() : null,
                message = new { role = messageRole, content = "..." }
            });
        }

        return new
        {
            id = root.TryGetProperty("id", out var id) ? id.GetString() : null,
            model = root.TryGetProperty("model", out var model) ? model.GetString() : null,
            created = root.TryGetProperty("created", out var created) ? created.GetInt64() : (long?)null,
            usage = root.TryGetProperty("usage", out var usage) ? usage : (JsonElement?)null,
            choices = newChoices
        };
    }

    private static object? RedactClaude(JsonElement root)
    {
        if (!root.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            return null;

        return new
        {
            id = root.TryGetProperty("id", out var id) ? id.GetString() : null,
            model = root.TryGetProperty("model", out var model) ? model.GetString() : null,
            type = root.TryGetProperty("type", out var type) ? type.GetString() : null,
            stop_reason = root.TryGetProperty("stop_reason", out var sr) ? sr.GetString() : null,
            usage = root.TryGetProperty("usage", out var usage) ? usage : (JsonElement?)null,
            content = new[] { new { type = "text", text = "..." } }
        };
    }

    private static object? RedactGemini(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var cands) || cands.ValueKind != JsonValueKind.Array)
            return null;

        var newCands = new List<object>();
        foreach (var c in cands.EnumerateArray())
        {
            newCands.Add(new
            {
                finishReason = c.TryGetProperty("finishReason", out var fr) ? fr.GetString() : null,
                safetyRatings = c.TryGetProperty("safetyRatings", out var sr) ? sr : (JsonElement?)null,
                content = new { role = "model", parts = new[] { new { text = "..." } } }
            });
        }

        return new
        {
            candidates = newCands,
            promptFeedback = root.TryGetProperty("promptFeedback", out var pf) ? pf : (JsonElement?)null,
            usageMetadata = root.TryGetProperty("usageMetadata", out var um) ? um : (JsonElement?)null
        };
    }

    private string GetApiKey(string providerId, string? authOverrideJson)
    {
        if (!string.IsNullOrWhiteSpace(authOverrideJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(authOverrideJson);
                if (doc.RootElement.TryGetProperty("apiKey", out var apiKey))
                    return apiKey.GetString() ?? "";
            }
            catch { }
        }

        return _settings.GetKeyForProvider(providerId, null);
    }

    private string GetModel(string providerId, string? authOverrideJson, string defaultModel)
    {
        if (!string.IsNullOrWhiteSpace(authOverrideJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(authOverrideJson);
                if (doc.RootElement.TryGetProperty("model", out var model))
                {
                    var v = model.GetString();
                    if (!string.IsNullOrWhiteSpace(v))
                        return v;
                }
            }
            catch { }
        }

        try
        {
            using var doc = JsonDocument.Parse(_settings.GetAuthJson(providerId));
            if (doc.RootElement.TryGetProperty("model", out var model))
            {
                var v = model.GetString();
                if (!string.IsNullOrWhiteSpace(v))
                    return v;
            }
        }
        catch { }

        return defaultModel;
    }

    private int GetMaxTokens(string providerId, string? authOverrideJson, int defaultMaxTokens = 2048)
    {
        if (!string.IsNullOrWhiteSpace(authOverrideJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(authOverrideJson);
                if (doc.RootElement.TryGetProperty("maxTokens", out var mt) && mt.ValueKind == JsonValueKind.Number)
                    return mt.GetInt32();
            }
            catch { }
        }

        try
        {
            using var doc = JsonDocument.Parse(_settings.GetAuthJson(providerId));
            if (doc.RootElement.TryGetProperty("maxTokens", out var mt) && mt.ValueKind == JsonValueKind.Number)
                return mt.GetInt32();
        }
        catch { }

        return defaultMaxTokens;
    }

    // ── Shared history builder ───────────────────────────────────────────────

    private static List<object> BuildOpenAiMessages(string providerId, string personalityMarkdown, string topic, List<SharedTurn> history)
        => BuildOpenAiStyleMessages(providerId, personalityMarkdown, topic, history);

    private static List<object> BuildDeepSeekMessages(string providerId, string personalityMarkdown, string topic, List<SharedTurn> history)
        => BuildOpenAiStyleMessages(providerId, personalityMarkdown, topic, history);

    /// <summary>
    /// Maximum number of recent turns to include in the context window.
    /// Only the most recent N turns are sent to keep token usage low.
    /// </summary>
    private const int MaxContextTurns = 8;

    private static List<SharedTurn> TrimHistory(List<SharedTurn> history)
    {
        if (history.Count <= MaxContextTurns)
            return history;
        return history.Skip(history.Count - MaxContextTurns).ToList();
    }

    private static List<object> BuildOpenAiStyleMessages(string providerId, string personalityMarkdown, string topic, List<SharedTurn> history)
    {
        var recent = TrimHistory(history);

        var messages = new List<object>
        {
            new { role = "system", content = $"{personalityMarkdown}\n\nTopic: \"{topic}\"" }
        };

        if (recent.Count == 0)
        {
            messages.Add(new { role = "user", content = $"The topic is: \"{topic}\". Please give your opening thoughts." });
            return messages;
        }

        foreach (var turn in recent)
        {
            if (turn.ModelId == providerId)
                messages.Add(new { role = "assistant", content = turn.Text });
            else
                messages.Add(new { role = "user", content = $"[{turn.ModelName}]: {turn.Text}" });
        }

        if (recent.Last().ModelId == providerId)
            messages.Add(new { role = "user", content = "Please continue the discussion." });

        return messages;
    }

    // ── OpenAI ───────────────────────────────────────────────────────────────

    private async Task<string> CallOpenAI(string providerId, string personalityMarkdown, string? authOverrideJson, string topic, List<SharedTurn> history)
    {
        var messages = BuildDeepSeekMessages(providerId, personalityMarkdown, topic, history);

        var model = GetModel("openai", authOverrideJson, defaultModel: "gpt-4o");

        var payload = new
        {
            model,
            max_tokens = GetMaxTokens("openai", authOverrideJson),
            messages
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey("openai", authOverrideJson));
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        EmitDiagnostics("openai", json, isError: !response.IsSuccessStatusCode);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"OpenAI {response.StatusCode}: {ExtractError(json)}");

        var doc = JsonDocument.Parse(json);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
        return SanitizeModelOutput(providerId, text);
    }

    // ── Claude ───────────────────────────────────────────────────────────────

    private async Task<string> CallClaude(string providerId, string personalityMarkdown, string? authOverrideJson, string topic, List<SharedTurn> history)
    {
        var openAiMessages = BuildOpenAiMessages(providerId, personalityMarkdown, topic, history);

        var system = $"{personalityMarkdown}\n\nTopic: \"{topic}\"";
        var filtered = openAiMessages
            .Where(m => (string)m.GetType().GetProperty("role")!.GetValue(m)! != "system")
            .Select(m => new
            {
                role = (string)m.GetType().GetProperty("role")!.GetValue(m)!,
                content = (string)m.GetType().GetProperty("content")!.GetValue(m)!
            })
            .ToList();

        var model = GetModel("claude", authOverrideJson, defaultModel: "claude-sonnet-4-6");

        var payload = new
        {
            model,
            max_tokens = GetMaxTokens("claude", authOverrideJson),
            system,
            messages = filtered
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", GetApiKey("claude", authOverrideJson));
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        EmitDiagnostics("claude", json, isError: !response.IsSuccessStatusCode);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Claude {response.StatusCode}: {ExtractError(json)}");

        var doc = JsonDocument.Parse(json);
        var text = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "";
        return SanitizeModelOutput(providerId, text);
    }

    // ── Gemini ───────────────────────────────────────────────────────────────

    private async Task<string> CallGemini(string providerId, string personalityMarkdown, string? authOverrideJson, string topic, List<SharedTurn> history)
    {
        var recent = TrimHistory(history);
        var rawTurns = new List<(string role, string text)>();

        if (recent.Count == 0)
        {
            rawTurns.Add(("user", $"The topic is: \"{topic}\". Please give your opening thoughts."));
        }
        else
        {
            foreach (var turn in recent)
            {
                var role = turn.ModelId == providerId ? "model" : "user";
                var text = turn.ModelId == providerId ? turn.Text : $"[{turn.ModelName}]: {turn.Text}";
                rawTurns.Add((role, text));
            }

            if (recent.Last().ModelId == providerId)
                rawTurns.Add(("user", "Please continue the discussion."));
        }

        var merged = new List<(string role, string text)>();
        foreach (var (role, text) in rawTurns)
        {
            if (merged.Count > 0 && merged.Last().role == role)
                merged[^1] = (role, merged.Last().text + "\n" + text);
            else
                merged.Add((role, text));
        }

        if (merged.Count > 0 && merged[0].role == "model")
            merged.Insert(0, ("user", $"Topic: \"{topic}\""));

        var contents = merged.Select(t => new
        {
            role = t.role,
            parts = new[] { new { text = t.text } }
        }).ToList();

        var payload = new
        {
            system_instruction = new { parts = new[] { new { text = $"{personalityMarkdown}\n\nTopic: \"{topic}\"\n\nRespond with a single cohesive message. Do not split your answer into separate parts." } } },
            contents,
            generationConfig = new { maxOutputTokens = GetMaxTokens("gemini", authOverrideJson) }
        };

        var model = GetModel("gemini", authOverrideJson, defaultModel: "gemini-2.0-flash-lite");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={GetApiKey("gemini", authOverrideJson)}";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        EmitDiagnostics("gemini", json, isError: !response.IsSuccessStatusCode);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini {response.StatusCode}: {ExtractError(json)}");

        var doc = JsonDocument.Parse(json);
        var parts = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts");

        if (parts.ValueKind != JsonValueKind.Array)
            return "";

        var sb = new StringBuilder();
        foreach (var p in parts.EnumerateArray())
        {
            if (!p.TryGetProperty("text", out var t))
                continue;

            var text = t.GetString();
            if (string.IsNullOrEmpty(text))
                continue;

            if (sb.Length > 0)
                sb.Append("\n");
            sb.Append(text);
        }

        return SanitizeModelOutput(providerId, sb.ToString());
    }

    // ── DeepSeek ─────────────────────────────────────────────────────────────

    private async Task<string> CallDeepSeek(string providerId, string personalityMarkdown, string? authOverrideJson, string topic, List<SharedTurn> history)
    {
        var messages = BuildOpenAiMessages(providerId, personalityMarkdown, topic, history);

        var model = GetModel("deepseek", authOverrideJson, defaultModel: "deepseek-chat");

        var payload = new
        {
            model,
            max_tokens = GetMaxTokens("deepseek", authOverrideJson),
            messages
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.deepseek.com/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetApiKey("deepseek", authOverrideJson));
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        EmitDiagnostics("deepseek", json, isError: !response.IsSuccessStatusCode);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"DeepSeek {response.StatusCode}: {ExtractError(json)}");

        var doc = JsonDocument.Parse(json);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
        return SanitizeModelOutput(providerId, text);
    }

    private static string SanitizeModelOutput(string providerId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Some models will incorrectly self-prefix turns with labels like "[ChatGPT]:".
        // Strip repeated instances at the start of the response.
        // Matches patterns like: "[ChatGPT]:", "ChatGPT:", "[Claude]:" etc.
        // Keeps the rest of the content intact.
        text = text.TrimStart();

        var pattern = "^(?:\\s*(?:\\[(?:chatgpt|gpt|assistant|openai|claude|gemini|deepseek)\\]\\s*:|(?:chatgpt|gpt|assistant|openai|claude|gemini|deepseek)\\s*:))+\\s*";
        text = System.Text.RegularExpressions.Regex.Replace(text, pattern, "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return text.TrimStart();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ExtractError(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var err))
            {
                if (err.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? json;
            }
        }
        catch { }
        return json.Length > 200 ? json[..200] : json;
    }
}
