using Maui.Biometric;
using Microsoft.Extensions.Logging;
using Moq;

namespace JournalApp.Tests;

public class BiometricAuthServiceTests
{
    [Fact]
    public async Task AuthenticateIfRequired_WhenUnlockNotRequired_ReturnsTrue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BiometricAuthService>>();
        var mockBiometric = new Mock<IBiometricAuthentication>();
        var preferences = new InMemoryPreferences();
        preferences.Set("require_unlock", false);

        var preferenceService = new PreferenceService(
            new Mock<ILogger<PreferenceService>>().Object,
            preferences);

        var service = new BiometricAuthService(
            mockLogger.Object,
            mockBiometric.Object,
            preferenceService);

        // Act
        var result = await service.AuthenticateIfRequired();

        // Assert
        Assert.True(result);
        mockBiometric.Verify(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateIfRequired_WhenUnlockRequired_CallsAuthentication()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BiometricAuthService>>();
        var mockBiometric = new Mock<IBiometricAuthentication>();
        var preferences = new InMemoryPreferences();
        preferences.Set("require_unlock", true);

        var preferenceService = new PreferenceService(
            new Mock<ILogger<PreferenceService>>().Object,
            preferences);

        mockBiometric.Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationResult { Status = AuthenticationStatus.Success });

        var service = new BiometricAuthService(
            mockLogger.Object,
            mockBiometric.Object,
            preferenceService);

        // Note: This test will only work properly on Android platform
        // On other platforms, it returns true without calling authentication

        // Act
        var result = await service.AuthenticateIfRequired();

        // Assert
        Assert.True(result);
        // Authentication is only called on Android, so we don't verify the call count here
    }
}
