namespace JournalApp.Tests.Data;

public class PreferenceServiceTests : JaTestContext
{
    [Fact]
    public void LastExportDate_WithMalformedStoredValue_ResetsToNowAndPersistsNewValue()
    {
        // Arrange
        var preferences = Services.GetService<IPreferences>();
        var preferenceService = Services.GetService<PreferenceService>();
        preferences.Set("last_export", "not-a-date");

        var before = DateTimeOffset.Now;

        // Act
        var result = preferenceService.LastExportDate;

        var after = DateTimeOffset.Now;

        // Assert
        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);

        var persisted = preferences.Get<string>("last_export", null);
        DateTimeOffset.TryParse(persisted, out var reparsed).Should().BeTrue();
        reparsed.Should().BeOnOrAfter(before);
        reparsed.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void SelectedAppTheme_WithInvalidStoredValue_FallsBackToUnspecified()
    {
        // Arrange
        var preferences = Services.GetService<IPreferences>();
        preferences.Set("theme", "DefinitelyNotATheme");

        var preferenceService = Services.GetService<PreferenceService>();

        // Act
        var result = preferenceService.SelectedAppTheme;

        // Assert
        result.Should().Be(AppTheme.Unspecified);
    }

    [Fact]
    public void SafetyPlan_WithMalformedJson_ReturnsNull()
    {
        // Arrange
        var preferences = Services.GetService<IPreferences>();
        preferences.Set("safety_plan", "{not valid json");

        var preferenceService = Services.GetService<PreferenceService>();

        // Act
        var result = preferenceService.SafetyPlan;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SafetyPlan_WithEmptyPlan_ClearsReadableValue()
    {
        // Arrange
        var preferences = Services.GetService<IPreferences>();
        var preferenceService = Services.GetService<PreferenceService>();

        // Act
        preferenceService.SafetyPlan = new SafetyPlan();

        // Assert
        preferences.Get<string>("safety_plan", "fallback").Should().BeNull();
        preferenceService.SafetyPlan.Should().BeNull();
    }

    [Fact]
    public void SelectedAppTheme_WithValidStoredValue_ParsesIt()
    {
        // Arrange - store the value before the service reads (and caches) it.
        var preferences = Services.GetService<IPreferences>();
        preferences.Set("theme", AppTheme.Dark.ToString());

        var preferenceService = Services.GetService<PreferenceService>();

        // Act & Assert
        preferenceService.SelectedAppTheme.Should().Be(AppTheme.Dark);
    }

    [Fact]
    public void LastExportDate_WithValidStoredValue_ReturnsStoredValue()
    {
        // Arrange
        var preferences = Services.GetService<IPreferences>();
        var stored = new DateTimeOffset(2024, 3, 15, 10, 0, 0, TimeSpan.Zero);
        preferences.Set("last_export", stored.ToString("O"));

        var preferenceService = Services.GetService<PreferenceService>();

        // Act & Assert - a valid stored date is returned as-is, not reset to now.
        preferenceService.LastExportDate.Should().Be(stored);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-mood")]
    [InlineData("🤔")] // present in DataPoint.Moods but excluded from the generated palette
    public void GetMoodColor_ReturnsTransparent_ForEmptyUnknownOrExcludedEmoji(string emoji)
    {
        var preferenceService = Services.GetService<PreferenceService>();

        preferenceService.GetMoodColor(emoji).Should().Be("transparent");
    }

    [Fact]
    public void GetMoodColor_ReturnsHexColor_ForKnownMood()
    {
        var preferenceService = Services.GetService<PreferenceService>();

        preferenceService.GetMoodColor("🤩").Should().StartWith("#");
    }
}
