using NUnit.Framework;
using LLMThinkTank.Core.Models;
using LLMThinkTank.Core.Services;

namespace LLMThinkTank.UnitTests;

[TestFixture]
public class AppearanceServiceTests
{
    private SettingsService _settings = null!;
    private AppearanceService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _settings = new SettingsService();
        _sut = new AppearanceService(_settings);
    }

    // ── Constructor ─────────────────────────────────────────────────────

    [Test]
    public void Constructor_LoadsFromSettings()
    {
        Assert.That(_sut.Mode, Is.TypeOf<AppearanceMode>());
        Assert.That(_sut.ControlHeight, Is.GreaterThanOrEqualTo(28).And.LessThanOrEqualTo(60));
        Assert.That(_sut.Gutter, Is.GreaterThanOrEqualTo(0).And.LessThanOrEqualTo(30));
        Assert.That(_sut.BorderRadius, Is.GreaterThanOrEqualTo(0).And.LessThanOrEqualTo(24));
    }

    // ── SetMode ─────────────────────────────────────────────────────────

    [Test]
    public void SetMode_ChangesMode()
    {
        _sut.SetMode(AppearanceMode.Neon);
        Assert.That(_sut.Mode, Is.EqualTo(AppearanceMode.Neon));
    }

    [Test]
    public void SetMode_FiresChangedEvent()
    {
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.SetMode(AppearanceMode.Matrix);
        Assert.That(fired, Is.True);
    }

    [Test]
    public void SetMode_SameValue_DoesNotFireChanged()
    {
        _sut.SetMode(AppearanceMode.Dark); // already Dark
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.SetMode(AppearanceMode.Dark);
        Assert.That(fired, Is.False);
    }

    [Test]
    public void SetMode_PersistsToSettings()
    {
        _sut.SetMode(AppearanceMode.Dracula);
        Assert.That(_settings.AppearanceTheme, Is.EqualTo("dracula"));
    }

    // ── SetControlHeight ────────────────────────────────────────────────

    [Test]
    public void SetControlHeight_ClampsToMin()
    {
        _sut.SetControlHeight(10);
        Assert.That(_sut.ControlHeight, Is.EqualTo(28));
    }

    [Test]
    public void SetControlHeight_ClampsToMax()
    {
        _sut.SetControlHeight(100);
        Assert.That(_sut.ControlHeight, Is.EqualTo(60));
    }

    [Test]
    public void SetControlHeight_AcceptsValidValue()
    {
        _sut.SetControlHeight(45);
        Assert.That(_sut.ControlHeight, Is.EqualTo(45));
    }

    [Test]
    public void SetControlHeight_SameValue_DoesNotFireChanged()
    {
        _sut.SetControlHeight(40); // default is 40
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.SetControlHeight(40);
        Assert.That(fired, Is.False);
    }

    [Test]
    public void SetControlHeight_FiresChangedEvent()
    {
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.SetControlHeight(50);
        Assert.That(fired, Is.True);
    }

    // ── SetGutter ───────────────────────────────────────────────────────

    [Test]
    public void SetGutter_ClampsToMin()
    {
        _sut.SetGutter(-5);
        Assert.That(_sut.Gutter, Is.EqualTo(0));
    }

    [Test]
    public void SetGutter_ClampsToMax()
    {
        _sut.SetGutter(50);
        Assert.That(_sut.Gutter, Is.EqualTo(30));
    }

    [Test]
    public void SetGutter_AcceptsValidValue()
    {
        _sut.SetGutter(15);
        Assert.That(_sut.Gutter, Is.EqualTo(15));
    }

    [Test]
    public void SetGutter_SameValue_DoesNotFireChanged()
    {
        _sut.SetGutter(15); // set to known value first
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.SetGutter(15); // same value again
        Assert.That(fired, Is.False);
    }

    // ── SetBorderRadius ─────────────────────────────────────────────────

    [Test]
    public void SetBorderRadius_ClampsToMin()
    {
        _sut.SetBorderRadius(-1);
        Assert.That(_sut.BorderRadius, Is.EqualTo(0));
    }

    [Test]
    public void SetBorderRadius_ClampsToMax()
    {
        _sut.SetBorderRadius(50);
        Assert.That(_sut.BorderRadius, Is.EqualTo(24));
    }

    [Test]
    public void SetBorderRadius_AcceptsValidValue()
    {
        _sut.SetBorderRadius(12);
        Assert.That(_sut.BorderRadius, Is.EqualTo(12));
    }

    [Test]
    public void SetBorderRadius_SameValue_DoesNotFireChanged()
    {
        _sut.SetBorderRadius(12); // set to known value first
        var fired = false;
        _sut.Changed += () => fired = true;
        _sut.SetBorderRadius(12); // same value again
        Assert.That(fired, Is.False);
    }

    // ── ToThemeValue ────────────────────────────────────────────────────

    [TestCase(AppearanceMode.Dark, "dark")]
    [TestCase(AppearanceMode.Light, "light")]
    [TestCase(AppearanceMode.Matrix, "matrix")]
    [TestCase(AppearanceMode.Neon, "neon")]
    [TestCase(AppearanceMode.Dracula, "dracula")]
    [TestCase(AppearanceMode.Aurora, "aurora")]
    [TestCase(AppearanceMode.Mono, "mono")]
    public void ToThemeValue_ReturnsLowercase(AppearanceMode mode, string expected)
    {
        Assert.That(AppearanceService.ToThemeValue(mode), Is.EqualTo(expected));
    }
}
