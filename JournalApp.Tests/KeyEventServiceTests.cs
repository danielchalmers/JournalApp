using Microsoft.Extensions.Logging.Abstractions;

namespace JournalApp.Tests;

/// <summary>
/// KeyEventService is the Android back-button action stack. Its peek-not-pop behaviour and the
/// empty-stack guard are the only things preventing the user from being trapped in a dialog or
/// unable to background the app, yet nothing else exercises them.
/// </summary>
public class KeyEventServiceTests
{
    private static KeyEventService NewService() => new(NullLogger<KeyEventService>.Instance);

    [Fact]
    public void Exited_OnEmptyStack_DoesNotThrowOrUnderflow()
    {
        var service = NewService();

        var act = () => service.Exited();

        act.Should().NotThrow();
        service.CurrentDepth.Should().Be(0);
    }

    [Fact]
    public void EnteredAndExited_TrackDepthAsLifo()
    {
        var service = NewService();

        service.Entered(() => { });
        service.Entered(() => { });
        service.CurrentDepth.Should().Be(2);

        service.Exited();
        service.CurrentDepth.Should().Be(1);
    }

    [Fact]
    public void ResetStack_ClearsAllActions()
    {
        var service = NewService();
        service.Entered(() => { });
        service.Entered(() => { });

        service.ResetStack();

        service.CurrentDepth.Should().Be(0);
    }

    [Fact]
    public void OnBackButtonPressed_InvokesOnlyTopActionAndReturnsTrue()
    {
        var service = NewService();
        var firstInvoked = false;
        var secondInvoked = false;
        service.Entered(() => firstInvoked = true);
        service.Entered(() => secondInvoked = true);

        var handled = service.OnBackButtonPressed();

        handled.Should().BeTrue();
        secondInvoked.Should().BeTrue();
        firstInvoked.Should().BeFalse();
    }

    [Fact]
    public void OnBackButtonPressed_DoesNotPopTheAction()
    {
        var service = NewService();
        var count = 0;
        service.Entered(() => count++);

        service.OnBackButtonPressed();
        service.OnBackButtonPressed();

        count.Should().Be(2);
        service.CurrentDepth.Should().Be(1);
    }

    [Fact]
    public void OnBackButtonPressed_ReturnsFalse_WhenStackEmpty()
    {
        var service = NewService();

        service.OnBackButtonPressed().Should().BeFalse();
    }
}
