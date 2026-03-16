using System.Collections.ObjectModel;
using LLMThinkTank.Core.Models;

namespace LLMThinkTank.Core.Services;

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
                convo.Topic = c.Topic;

                if (c.Messages is not null)
                    convo.Messages.AddRange(c.Messages);

                if (c.StatusEvents is not null)
                    convo.StatusEvents.AddRange(c.StatusEvents);

                if (c.Diagnostics is not null)
                    convo.Diagnostics.AddRange(c.Diagnostics);

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

        s.SetConversations(Conversations.Select(c => new PersistedConversation(
            c.ChatId,
            c.Title,
            c.Participants.Select(p => new PersistedParticipant(
                p.ParticipantId,
                p.TemplateId,
                p.ProviderId,
                p.DisplayName,
                p.PersonalityMarkdown,
                p.AuthOverrideJson)).ToList(),
            Topic: c.Topic,
            Messages: c.Messages.Count == 0 ? null : new List<PersistedMessage>(c.Messages))
        {
            StatusEvents = c.StatusEvents.Count == 0 ? null : new List<PersistedStatusEvent>(c.StatusEvents)
            ,
            Diagnostics = c.Diagnostics.Count == 0 ? null : new List<PersistedStatusEvent>(c.Diagnostics)
        }));
    }
}
