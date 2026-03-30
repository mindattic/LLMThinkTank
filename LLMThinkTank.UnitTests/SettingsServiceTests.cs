using NUnit.Framework;
using LLMThinkTank.Core.Models;
using LLMThinkTank.Core.Services;

namespace LLMThinkTank.UnitTests;

[TestFixture]
public class SettingsServiceTests
{
    private SettingsService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new SettingsService();
    }

    // ── GetAuthJson ─────────────────────────────────────────────────────

    [Test]
    public void GetAuthJson_KnownProvider_ReturnsJson()
    {
        var json = _sut.GetAuthJson("openai");
        Assert.That(json, Does.Contain("apiKey"));
    }

    [Test]
    public void GetAuthJson_UnknownProvider_ReturnsEmptyObject()
    {
        var json = _sut.GetAuthJson("nonexistent_provider");
        Assert.That(json, Is.EqualTo("{}"));
    }

    [Test]
    public void GetAuthJson_AllDefaultProviders_ReturnValidJson()
    {
        var providers = new[] { "openai", "claude", "gemini", "deepseek", "mistral", "xai", "groq", "together", "openrouter", "fireworks", "cohere" };
        foreach (var p in providers)
        {
            var json = _sut.GetAuthJson(p);
            Assert.DoesNotThrow(() => System.Text.Json.JsonDocument.Parse(json), $"Invalid JSON for provider {p}");
        }
    }

    // ── GetKeyForProvider ───────────────────────────────────────────────

    [Test]
    public void GetKeyForProvider_WithOverride_ReturnsOverride()
    {
        var key = _sut.GetKeyForProvider("openai", "my-override-key");
        Assert.That(key, Is.EqualTo("my-override-key"));
    }

    [Test]
    public void GetKeyForProvider_NullOverride_ExtractsFromAuthJson()
    {
        _sut.ProviderAuth["testprov"] = new ProviderAuthConfig("testprov",
            "{\"type\":\"bearer\",\"apiKey\":\"sk-test-123\"}");

        var key = _sut.GetKeyForProvider("testprov", null);
        Assert.That(key, Is.EqualTo("sk-test-123"));
    }

    [Test]
    public void GetKeyForProvider_EmptyOverride_ExtractsFromAuthJson()
    {
        _sut.ProviderAuth["testprov"] = new ProviderAuthConfig("testprov",
            "{\"type\":\"bearer\",\"apiKey\":\"sk-test-456\"}");

        var key = _sut.GetKeyForProvider("testprov", "");
        Assert.That(key, Is.EqualTo("sk-test-456"));
    }

    [Test]
    public void GetKeyForProvider_MissingProvider_ReturnsEmpty()
    {
        var key = _sut.GetKeyForProvider("nonexistent", null);
        Assert.That(key, Is.EqualTo(""));
    }

    [Test]
    public void GetKeyForProvider_MalformedJson_ReturnsEmpty()
    {
        _sut.ProviderAuth["bad"] = new ProviderAuthConfig("bad", "not json");
        var key = _sut.GetKeyForProvider("bad", null);
        Assert.That(key, Is.EqualTo(""));
    }

    // ── ProviderAuth initialization ─────────────────────────────────────

    [Test]
    public void ProviderAuth_Has11Providers()
    {
        Assert.That(_sut.ProviderAuth.Count, Is.EqualTo(11));
    }

    [TestCase("openai")]
    [TestCase("claude")]
    [TestCase("gemini")]
    [TestCase("deepseek")]
    [TestCase("mistral")]
    [TestCase("xai")]
    [TestCase("groq")]
    [TestCase("together")]
    [TestCase("openrouter")]
    [TestCase("fireworks")]
    [TestCase("cohere")]
    public void ProviderAuth_ContainsExpectedProvider(string providerId)
    {
        Assert.That(_sut.ProviderAuth.ContainsKey(providerId), Is.True);
    }

    [Test]
    public void ProviderAuth_AllContainMaxTokens()
    {
        foreach (var (providerId, cfg) in _sut.ProviderAuth)
        {
            var doc = System.Text.Json.JsonDocument.Parse(cfg.Json);
            Assert.That(doc.RootElement.TryGetProperty("maxTokens", out _), Is.True,
                $"Provider {providerId} missing maxTokens");
        }
    }

    [Test]
    public void ProviderAuth_AllContainModel()
    {
        foreach (var (providerId, cfg) in _sut.ProviderAuth)
        {
            var doc = System.Text.Json.JsonDocument.Parse(cfg.Json);
            Assert.That(doc.RootElement.TryGetProperty("model", out _), Is.True,
                $"Provider {providerId} missing model");
        }
    }

    // ── Templates ───────────────────────────────────────────────────────

    [Test]
    public void Templates_HasAtLeast11Templates()
    {
        Assert.That(_sut.Templates.Count, Is.GreaterThanOrEqualTo(11));
    }

    [Test]
    public void Templates_ContainsDefaultTemplates()
    {
        Assert.That(_sut.Templates.Count(t => t.IsDefault), Is.GreaterThanOrEqualTo(11));
    }

    [Test]
    public void Templates_AllHaveUniqueIds()
    {
        var ids = _sut.Templates.Select(t => t.TemplateId).ToHashSet();
        Assert.That(ids.Count, Is.EqualTo(_sut.Templates.Count));
    }

    [Test]
    public void Templates_CoverAllProviders()
    {
        var providers = _sut.Templates.Select(t => t.ProviderId).ToHashSet();
        var expected = new[] { "openai", "claude", "gemini", "deepseek", "mistral", "xai", "groq", "together", "openrouter", "fireworks", "cohere" };
        foreach (var p in expected)
            Assert.That(providers.Contains(p), Is.True, $"Missing template for provider: {p}");
    }

    // ── Default settings (loaded from disk or initialized) ────────────

    [Test]
    public void AppearanceTheme_IsNotNullOrEmpty()
    {
        Assert.That(_sut.AppearanceTheme, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void GlobalMaxTokens_HasValue()
    {
        Assert.That(_sut.GlobalMaxTokens, Is.Not.Null);
        Assert.That(_sut.GlobalMaxTokens, Is.GreaterThan(0));
    }

    [Test]
    public void GlobalMaxRounds_HasValue()
    {
        Assert.That(_sut.GlobalMaxRounds, Is.Not.Null);
        Assert.That(_sut.GlobalMaxRounds, Is.GreaterThan(0));
    }

    [Test]
    public void ControlHeight_HasValue()
    {
        Assert.That(_sut.ControlHeight, Is.Not.Null);
        Assert.That(_sut.ControlHeight, Is.GreaterThanOrEqualTo(28));
    }

    [Test]
    public void Gutter_HasValue()
    {
        Assert.That(_sut.Gutter, Is.Not.Null);
        Assert.That(_sut.Gutter, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void BorderRadius_HasValue()
    {
        Assert.That(_sut.BorderRadius, Is.Not.Null);
        Assert.That(_sut.BorderRadius, Is.GreaterThanOrEqualTo(0));
    }

    // ── SetAuthJson / GetAuthJson roundtrip ─────────────────────────────

    [Test]
    public void SetAuthJson_ThenGetAuthJson_Roundtrips()
    {
        var json = "{\"type\":\"bearer\",\"apiKey\":\"test-key\",\"model\":\"gpt-4\"}";
        _sut.SetAuthJson("testprov_roundtrip", json);

        var result = _sut.GetAuthJson("testprov_roundtrip");
        Assert.That(result, Is.EqualTo(json));

        // Cleanup
        _sut.ProviderAuth.Remove("testprov_roundtrip");
    }

    // ── SetKey ──────────────────────────────────────────────────────────

    [Test]
    public void SetKey_UpdatesApiKeyInAuthJson()
    {
        _sut.SetKey("openai", "sk-new-key-123");

        var json = _sut.GetAuthJson("openai");
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var apiKey = doc.RootElement.GetProperty("apiKey").GetString();
        Assert.That(apiKey, Is.EqualTo("sk-new-key-123"));
    }

    [Test]
    public void SetKey_PreservesModelSetting()
    {
        var originalJson = _sut.GetAuthJson("openai");
        var originalDoc = System.Text.Json.JsonDocument.Parse(originalJson);
        var originalModel = originalDoc.RootElement.GetProperty("model").GetString();

        _sut.SetKey("openai", "new-key");

        var updatedJson = _sut.GetAuthJson("openai");
        var updatedDoc = System.Text.Json.JsonDocument.Parse(updatedJson);
        var updatedModel = updatedDoc.RootElement.GetProperty("model").GetString();

        Assert.That(updatedModel, Is.EqualTo(originalModel));
    }

    [Test]
    public void SetKey_PreservesTypeSetting()
    {
        _sut.SetKey("claude", "new-key");

        var json = _sut.GetAuthJson("claude");
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var type = doc.RootElement.GetProperty("type").GetString();
        Assert.That(type, Is.EqualTo("anthropic"));
    }

    // ── ResetProvidersToDefaults ────────────────────────────────────────

    [Test]
    public void ResetProvidersToDefaults_EmptyDefaults_IsNoOp()
    {
        Assert.That(_sut.ProviderDefaults.Count, Is.EqualTo(0));
        var beforeCount = _sut.ProviderAuth.Count;

        _sut.ResetProvidersToDefaults();

        Assert.That(_sut.ProviderAuth.Count, Is.EqualTo(beforeCount));
    }

    [Test]
    public void ResetProvidersToDefaults_AppliesDefaults()
    {
        _sut.ProviderDefaults["openai"] = new ProviderAuthConfig("openai",
            "{\"type\":\"bearer\",\"apiKey\":\"default-key\",\"model\":\"gpt-4\"}");

        _sut.ResetProvidersToDefaults();

        var json = _sut.GetAuthJson("openai");
        Assert.That(json, Does.Contain("default-key"));
    }

    // ── SetConversations ────────────────────────────────────────────────

    [Test]
    public void SetConversations_ReplacesAll()
    {
        var convos = new List<PersistedConversation>
        {
            new("id1", "Title1", new List<PersistedParticipant>(), null, null),
            new("id2", "Title2", new List<PersistedParticipant>(), null, null)
        };

        _sut.SetConversations(convos);
        Assert.That(_sut.Conversations, Has.Count.EqualTo(2));
    }

    [Test]
    public void SetConversations_EmptyList_ClearsAll()
    {
        _sut.SetConversations(new[]
        {
            new PersistedConversation("id1", "Title1", new List<PersistedParticipant>(), null, null)
        });

        _sut.SetConversations(Enumerable.Empty<PersistedConversation>());
        Assert.That(_sut.Conversations, Is.Empty);
    }

    // ── SaveTemplates ───────────────────────────────────────────────────

    [Test]
    public void SaveTemplates_ReplacesAll()
    {
        var templates = new List<ParticipantTemplate>
        {
            new("t1", "openai", "Custom GPT", "Custom personality", null, false)
        };

        _sut.SaveTemplates(templates);
        Assert.That(_sut.Templates, Has.Count.EqualTo(1));
        Assert.That(_sut.Templates[0].DisplayName, Is.EqualTo("Custom GPT"));
    }

    // ── Appearance settings mutations ───────────────────────────────────

    [Test]
    public void SetAppearanceTheme_Updates()
    {
        _sut.SetAppearanceTheme("neon");
        Assert.That(_sut.AppearanceTheme, Is.EqualTo("neon"));
    }

    [Test]
    public void SetAppearanceTheme_BlankDefaultsToDark()
    {
        _sut.SetAppearanceTheme("");
        Assert.That(_sut.AppearanceTheme, Is.EqualTo("dark"));
    }

    [Test]
    public void SetAppearanceTheme_WhitespaceDefaultsToDark()
    {
        _sut.SetAppearanceTheme("   ");
        Assert.That(_sut.AppearanceTheme, Is.EqualTo("dark"));
    }

    [Test]
    public void SetControlHeight_Updates()
    {
        _sut.SetControlHeight(50);
        Assert.That(_sut.ControlHeight, Is.EqualTo(50));
    }

    [Test]
    public void SetGutter_Updates()
    {
        _sut.SetGutter(20);
        Assert.That(_sut.Gutter, Is.EqualTo(20));
    }

    [Test]
    public void SetBorderRadius_Updates()
    {
        _sut.SetBorderRadius(16);
        Assert.That(_sut.BorderRadius, Is.EqualTo(16));
    }

    [Test]
    public void SetGlobalMaxTokens_Updates()
    {
        _sut.SetGlobalMaxTokens(4096);
        Assert.That(_sut.GlobalMaxTokens, Is.EqualTo(4096));
    }

    [Test]
    public void SetGlobalMaxRounds_Updates()
    {
        _sut.SetGlobalMaxRounds(5);
        Assert.That(_sut.GlobalMaxRounds, Is.EqualTo(5));
    }
}
