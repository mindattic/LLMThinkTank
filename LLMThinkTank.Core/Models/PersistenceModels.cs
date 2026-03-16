namespace LLMThinkTank.Core.Models;

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
