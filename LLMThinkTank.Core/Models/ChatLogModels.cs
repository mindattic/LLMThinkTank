namespace LLMThinkTank.Core.Models;

public record ChatLogEntry(DateTimeOffset Timestamp, string Source, string Text, bool IsError = false);

public sealed record PersistedTurn(string ParticipantId, string Text, int Round, bool IsError);
