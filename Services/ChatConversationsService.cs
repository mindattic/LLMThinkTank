using System.Collections.ObjectModel;

namespace  LLMThinkTank.Services;

public record ParticipantTemplate(
    string TemplateId,
    string ProviderId,
    string DisplayName,
    string PersonalityMarkdown,
    string? AuthOverrideJson);

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

    public ChatConversation(string chatId, string title)
    {
        ChatId = chatId;
        Title = title;
    }
}

public class ChatConversationsService
{
    private readonly LlmThinkTankSettingsService _settings;

    public ObservableCollection<ChatConversation> Conversations { get; } = new();

    public ChatConversation? ActiveConversation { get; private set; }

    public event Action? Changed;

    public ChatConversationsService(LlmThinkTankSettingsService settings)
    {
        _settings = settings;

        if (_settings is SettingsService s)
        {
            foreach (var c in s.Conversations)
            {
                var convo = new ChatConversation(c.ChatId, c.Title);
                foreach (var p in c.Participants)
                {
                    convo.Participants.Add(new ChatParticipant(
                        p.ParticipantId,
                        p.TemplateId,
                        p.ProviderId,
                        p.DisplayName,
                        p.PersonalityMarkdown,
                        p.AuthOverrideJson));
                }

                Conversations.Add(convo);
            }

            ActiveConversation = Conversations.LastOrDefault();
        }
    }

    public static string NewId() => Guid.NewGuid().ToString("N");

    public void SetActive(string chatId)
    {
        ActiveConversation = Conversations.FirstOrDefault(c => c.ChatId == chatId);
        Persist();
        Changed?.Invoke();
    }

    public ChatConversation CreateConversation(string title)
    {
        var chatId = NewId();
        var convo = new ChatConversation(chatId, title);
        Conversations.Add(convo);
        ActiveConversation = convo;
        Persist();
        Changed?.Invoke();
        return convo;
    }

    public void CloseConversation(string chatId)
    {
        var convo = Conversations.FirstOrDefault(c => c.ChatId == chatId);
        if (convo is null)
            return;

        Conversations.Remove(convo);
        if (ActiveConversation?.ChatId == chatId)
            ActiveConversation = Conversations.LastOrDefault();
        Persist();
        Changed?.Invoke();
    }

    public void NotifyChanged()
    {
        Persist();
        Changed?.Invoke();
    }

    private void Persist()
    {
        if (_settings is not SettingsService s)
            return;

        s.SetConversations(Conversations.Select(c => new SettingsService.PersistedConversation(
            c.ChatId,
            c.Title,
            c.Participants.Select(p => new SettingsService.PersistedParticipant(
                p.ParticipantId,
                p.TemplateId,
                p.ProviderId,
                p.DisplayName,
                p.PersonalityMarkdown,
                p.AuthOverrideJson)).ToList(),
            Topic: null)));
    }
}
