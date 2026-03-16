using System.Collections.ObjectModel;

namespace LLMThinkTank.Core.Models;

public record ParticipantTemplate(
    string TemplateId,
    string ProviderId,
    string DisplayName,
    string PersonalityMarkdown,
    string? AuthOverrideJson,
    bool IsDefault = false);

public record ChatParticipant(
    string ParticipantId,
    string TemplateId,
    string ProviderId,
    string DisplayName,
    string PersonalityMarkdown,
    string? AuthOverrideJson);

public class ChatConversation
{
    public string ChatId { get; }
    public string Title { get; set; }
    public ObservableCollection<ChatParticipant> Participants { get; } = new();
    public string? Topic { get; set; }

    public List<PersistedMessage> Messages { get; } = new();
    public List<PersistedStatusEvent> StatusEvents { get; } = new();
    public List<PersistedStatusEvent> Diagnostics { get; } = new();

    public ChatConversation(string chatId, string title)
    {
        ChatId = chatId;
        Title = title;
    }
}
