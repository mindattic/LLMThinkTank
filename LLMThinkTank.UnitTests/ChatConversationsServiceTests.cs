using NUnit.Framework;
using LLMThinkTank.Core.Models;
using LLMThinkTank.Core.Services;

namespace LLMThinkTank.UnitTests;

[TestFixture]
public class ChatConversationsServiceTests
{
    private SettingsService _settings = null!;
    private ChatConversationsService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _settings = new SettingsService();
        _settings.SetConversations(Enumerable.Empty<PersistedConversation>());
        _sut = new ChatConversationsService(_settings);
    }

    // ── NewId ───────────────────────────────────────────────────────────

    [Test]
    public void NewId_Returns32CharHexString()
    {
        var id = ChatConversationsService.NewId();
        Assert.That(id, Has.Length.EqualTo(32));
        Assert.That(id.All(c => "0123456789abcdef".Contains(c)), Is.True, $"'{id}' is not hex");
    }

    [Test]
    public void NewId_ReturnsUniqueValues()
    {
        var ids = Enumerable.Range(0, 100).Select(_ => ChatConversationsService.NewId()).ToHashSet();
        Assert.That(ids.Count, Is.EqualTo(100));
    }

    // ── CreateConversation ──────────────────────────────────────────────

    [Test]
    public void CreateConversation_AddsToCollection()
    {
        _sut.CreateConversation("Test");
        Assert.That(_sut.Conversations, Has.Count.EqualTo(1));
    }

    [Test]
    public void CreateConversation_SetsTitle()
    {
        var convo = _sut.CreateConversation("My Topic");
        Assert.That(convo.Title, Is.EqualTo("My Topic"));
    }

    [Test]
    public void CreateConversation_SetsAsActive()
    {
        var convo = _sut.CreateConversation("Active");
        Assert.That(_sut.ActiveConversation, Is.SameAs(convo));
    }

    [Test]
    public void CreateConversation_FiresChangedEvent()
    {
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.CreateConversation("Test");
        Assert.That(fired, Is.True);
    }

    [Test]
    public void CreateConversation_AssignsChatId()
    {
        var convo = _sut.CreateConversation("Test");
        Assert.That(convo.ChatId, Is.Not.Null.And.Not.Empty);
        Assert.That(convo.ChatId, Has.Length.EqualTo(32));
    }

    [Test]
    public void CreateConversation_Multiple_LastIsActive()
    {
        _sut.CreateConversation("First");
        var second = _sut.CreateConversation("Second");
        Assert.That(_sut.ActiveConversation, Is.SameAs(second));
        Assert.That(_sut.Conversations, Has.Count.EqualTo(2));
    }

    // ── SetActive ───────────────────────────────────────────────────────

    [Test]
    public void SetActive_SwitchesToCorrectConversation()
    {
        var first = _sut.CreateConversation("First");
        _sut.CreateConversation("Second");

        _sut.SetActive(first.ChatId);
        Assert.That(_sut.ActiveConversation, Is.SameAs(first));
    }

    [Test]
    public void SetActive_UnknownId_SetsActiveToNull()
    {
        _sut.CreateConversation("Test");
        _sut.SetActive("nonexistent");
        Assert.That(_sut.ActiveConversation, Is.Null);
    }

    [Test]
    public void SetActive_FiresChangedEvent()
    {
        var convo = _sut.CreateConversation("Test");
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.SetActive(convo.ChatId);
        Assert.That(fired, Is.True);
    }

    // ── CloseConversation ───────────────────────────────────────────────

    [Test]
    public void CloseConversation_RemovesFromCollection()
    {
        var convo = _sut.CreateConversation("ToClose");
        _sut.CloseConversation(convo.ChatId);
        Assert.That(_sut.Conversations, Is.Empty);
    }

    [Test]
    public void CloseConversation_ActiveClosedUpdatesToLast()
    {
        var first = _sut.CreateConversation("First");
        var second = _sut.CreateConversation("Second");

        _sut.CloseConversation(second.ChatId);
        Assert.That(_sut.ActiveConversation, Is.SameAs(first));
    }

    [Test]
    public void CloseConversation_ActiveClosed_NoRemaining_SetsNull()
    {
        var convo = _sut.CreateConversation("Only");
        _sut.CloseConversation(convo.ChatId);
        Assert.That(_sut.ActiveConversation, Is.Null);
    }

    [Test]
    public void CloseConversation_NonActive_DoesNotChangeActive()
    {
        var first = _sut.CreateConversation("First");
        var second = _sut.CreateConversation("Second");

        _sut.CloseConversation(first.ChatId);
        Assert.That(_sut.ActiveConversation, Is.SameAs(second));
    }

    [Test]
    public void CloseConversation_UnknownId_IsNoOp()
    {
        _sut.CreateConversation("Keep");
        _sut.CloseConversation("bogus");
        Assert.That(_sut.Conversations, Has.Count.EqualTo(1));
    }

    [Test]
    public void CloseConversation_FiresChangedEvent()
    {
        var convo = _sut.CreateConversation("Test");
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.CloseConversation(convo.ChatId);
        Assert.That(fired, Is.True);
    }

    // ── NotifyChanged ───────────────────────────────────────────────────

    [Test]
    public void NotifyChanged_FiresChangedEvent()
    {
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.NotifyChanged();
        Assert.That(fired, Is.True);
    }

    // ── Rehydration from settings ───────────────────────────────────────

    [Test]
    public void Constructor_EmptySettings_NoConversations()
    {
        Assert.That(_sut.Conversations, Is.Empty);
        Assert.That(_sut.ActiveConversation, Is.Null);
    }
}
