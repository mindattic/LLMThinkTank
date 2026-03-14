using System.Text.Json;

namespace  LLMThinkTank.Services;

public record ChatLogEntry(DateTimeOffset Timestamp, string Source, string Text, bool IsError = false);

public class ChatLogService
{
    private readonly List<ChatLogEntry> _entries = new();

    public event Action? Changed;

    public IReadOnlyList<ChatLogEntry> Entries => _entries;

    public void Add(string source, string text, bool isError = false)
    {
        _entries.Add(new ChatLogEntry(DateTimeOffset.UtcNow, source, text, isError));
        Changed?.Invoke();
    }

    public void Clear()
    {
        _entries.Clear();
        Changed?.Invoke();
    }
}

public static class ChatStorage
{
    public static string GetChatFolder(string chatId)
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MindAttic", "LLMThinkTank", "Conversations", chatId);

    public static string GetChatJsonPath(string chatId)
        => Path.Combine(GetChatFolder(chatId), "chat.json");

    public static string GetPerspectivePath(string chatId, string modelId)
        => Path.Combine(GetChatFolder(chatId), $"{modelId}.md");

    public static async Task AppendChatJsonAsync(string chatId, object entry, CancellationToken cancellationToken = default)
    {
        var folder = GetChatFolder(chatId);
        Directory.CreateDirectory(folder);

        var path = GetChatJsonPath(chatId);

        List<JsonElement> items;
        if (File.Exists(path))
        {
            await using var read = File.OpenRead(path);
            items = await JsonSerializer.DeserializeAsync<List<JsonElement>>(read, cancellationToken: cancellationToken) ?? new();
        }
        else
        {
            items = new();
        }

        var elem = JsonSerializer.SerializeToElement(entry);
        items.Add(elem);

        await using var write = File.Create(path);
        await JsonSerializer.SerializeAsync(write, items, new JsonSerializerOptions { WriteIndented = true }, cancellationToken);
    }

    public static async Task<string> ReadPerspectiveAsync(string chatId, string modelId, CancellationToken cancellationToken = default)
    {
        var path = GetPerspectivePath(chatId, modelId);
        if (!File.Exists(path))
            return "";

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public static Task WritePerspectiveAsync(string chatId, string modelId, string markdown, CancellationToken cancellationToken = default)
    {
        var folder = GetChatFolder(chatId);
        Directory.CreateDirectory(folder);

        var path = GetPerspectivePath(chatId, modelId);
        return File.WriteAllTextAsync(path, markdown, cancellationToken);
    }

    public static async Task<List<PersistedTurn>> LoadTurnsAsync(string chatId, CancellationToken cancellationToken = default)
    {
        var path = GetChatJsonPath(chatId);
        if (!File.Exists(path))
            return new();

        await using var read = File.OpenRead(path);
        var items = await JsonSerializer.DeserializeAsync<List<JsonElement>>(read, cancellationToken: cancellationToken) ?? new();

        var turns = new List<PersistedTurn>();
        foreach (var item in items)
        {
            if (!item.TryGetProperty("type", out var type) || type.GetString() != "turn")
                continue;

            var participantId = item.TryGetProperty("participantId", out var pid) ? pid.GetString() ?? "" : "";
            var text = item.TryGetProperty("text", out var textElem) ? textElem.GetString() ?? "" : "";
            var round = item.TryGetProperty("round", out var roundElem) ? roundElem.GetInt32() : 0;
            var isError = item.TryGetProperty("isError", out var errElem) && errElem.ValueKind == JsonValueKind.True;

            turns.Add(new PersistedTurn(participantId, text, round, isError));
        }

        return turns;
    }

    public sealed record PersistedTurn(string ParticipantId, string Text, int Round, bool IsError);
}
