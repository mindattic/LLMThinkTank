using NUnit.Framework;
using LLMThinkTank.Core.Services;

namespace LLMThinkTank.UnitTests;

[TestFixture]
public class ChatLogServiceTests
{
    private ChatLogService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ChatLogService();
    }

    [Test]
    public void Entries_InitiallyEmpty()
    {
        Assert.That(_sut.Entries, Is.Empty);
    }

    [Test]
    public void Add_AppendsEntry()
    {
        _sut.Add("openai", "test message");
        Assert.That(_sut.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void Add_SetsCorrectProperties()
    {
        _sut.Add("claude", "some text", isError: true);

        var entry = _sut.Entries[0];
        Assert.That(entry.Source, Is.EqualTo("claude"));
        Assert.That(entry.Text, Is.EqualTo("some text"));
        Assert.That(entry.IsError, Is.True);
        Assert.That(entry.Timestamp, Is.Not.EqualTo(default(DateTimeOffset)));
    }

    [Test]
    public void Add_DefaultIsErrorFalse()
    {
        _sut.Add("system", "info");
        Assert.That(_sut.Entries[0].IsError, Is.False);
    }

    [Test]
    public void Add_FiresChangedEvent()
    {
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.Add("test", "msg");
        Assert.That(fired, Is.True);
    }

    [Test]
    public void Add_MultipleEntries_PreservesOrder()
    {
        _sut.Add("a", "first");
        _sut.Add("b", "second");
        _sut.Add("c", "third");

        Assert.That(_sut.Entries, Has.Count.EqualTo(3));
        Assert.That(_sut.Entries[0].Source, Is.EqualTo("a"));
        Assert.That(_sut.Entries[1].Source, Is.EqualTo("b"));
        Assert.That(_sut.Entries[2].Source, Is.EqualTo("c"));
    }

    [Test]
    public void Clear_RemovesAllEntries()
    {
        _sut.Add("a", "1");
        _sut.Add("b", "2");
        _sut.Clear();

        Assert.That(_sut.Entries, Is.Empty);
    }

    [Test]
    public void Clear_FiresChangedEvent()
    {
        _sut.Add("x", "y");
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.Clear();
        Assert.That(fired, Is.True);
    }

    [Test]
    public void Clear_OnEmpty_StillFiresChanged()
    {
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.Clear();
        Assert.That(fired, Is.True);
    }

    [Test]
    public void Add_SetsTimestampNearNow()
    {
        var before = DateTimeOffset.UtcNow;
        _sut.Add("test", "msg");
        var after = DateTimeOffset.UtcNow;

        Assert.That(_sut.Entries[0].Timestamp, Is.GreaterThanOrEqualTo(before));
        Assert.That(_sut.Entries[0].Timestamp, Is.LessThanOrEqualTo(after));
    }
}
