using NUnit.Framework;
using LLMThinkTank.Core.Services;

namespace LLMThinkTank.UnitTests;

[TestFixture]
public class HumanNameServiceTests
{
    private HumanNameService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new HumanNameService();
    }

    [Test]
    public void NextFirstName_ReturnsNonNullNonEmpty()
    {
        var name = _sut.NextFirstName();
        Assert.That(name, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void NextFirstName_ReturnsOnlyLetters()
    {
        for (var i = 0; i < 100; i++)
        {
            var name = _sut.NextFirstName();
            Assert.That(name.All(char.IsLetter), Is.True, $"Name '{name}' contains non-letter characters");
        }
    }

    [Test]
    public void NextFirstName_ReturnsVariousNames()
    {
        var names = new HashSet<string>();
        for (var i = 0; i < 200; i++)
            names.Add(_sut.NextFirstName());

        Assert.That(names.Count, Is.GreaterThan(5), "Expected multiple distinct names from 200 samples");
    }

    [Test]
    public void NextDisplayName_FormatsCorrectly()
    {
        var displayName = _sut.NextDisplayName("ChatGPT");
        Assert.That(displayName, Does.EndWith(" (ChatGPT)"));
        Assert.That(displayName.Length, Is.GreaterThan("(ChatGPT)".Length + 1));
    }

    [Test]
    public void NextDisplayName_ContainsParenthesizedLlmName()
    {
        var displayName = _sut.NextDisplayName("Claude");
        Assert.That(displayName, Does.Contain("(Claude)"));
    }

    [Test]
    public void NextDisplayName_StartsWithFirstName()
    {
        var displayName = _sut.NextDisplayName("Gemini");
        var firstNamePart = displayName.Split(' ')[0];
        Assert.That(firstNamePart.All(char.IsLetter), Is.True);
    }
}
