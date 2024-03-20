namespace JournalApp.Tests;

public class SafetyPlanTests : JaTestContext
{
    [Fact]
    public void Persists()
    {
        var preferenceService = Services.GetService<PreferenceService>();

        preferenceService.SafetyPlan = new()
        {
            Purpose = "My purpose is to finish these tests",
        };

        var cut = RenderComponent<SafetyPlanPage>();

        // Assert initial state.
        cut.Instance.Plan.Purpose.Should().Be("My purpose is to finish these tests");
        cut.Find(".safety-plan-item-purpose textarea").TextContent.Should().Be("My purpose is to finish these tests");

        // Change via textarea and property to test both.
        cut.Find(".safety-plan-item-purpose textarea").Input("Actually it's puppies");
        cut.Instance.Plan.ProfessionalContacts = "988 (USA)";

        // Leave and come back.
        DisposeComponents();
        cut = RenderComponent<SafetyPlanPage>();

        // Assert that all changes persisted.
        preferenceService.SafetyPlan.Purpose.Should().Be("Actually it's puppies");
        cut.Find(".safety-plan-item-purpose textarea").TextContent.Should().Be("Actually it's puppies");
        cut.Find(".safety-plan-item-professional-supports textarea").TextContent.Should().Be("988 (USA)");
    }
}
