using Maui.Biometric;

namespace JournalApp;

/// <summary>
/// Service for handling biometric authentication on Android.
/// </summary>
public sealed class BiometricAuthService
{
    private readonly ILogger<BiometricAuthService> _logger;
    private readonly IBiometricAuthentication _biometricService;
    private readonly PreferenceService _preferences;

    public BiometricAuthService(
        ILogger<BiometricAuthService> logger,
        IBiometricAuthentication biometricService,
        PreferenceService preferences)
    {
        _logger = logger;
        _biometricService = biometricService;
        _preferences = preferences;
    }

    /// <summary>
    /// Attempts to authenticate the user using biometric authentication if required.
    /// </summary>
    /// <returns>True if authentication was successful or not required, false otherwise.</returns>
    public async Task<bool> AuthenticateIfRequired()
    {
        if (!_preferences.RequireUnlock)
        {
            _logger.LogDebug("Biometric unlock is not required");
            return true;
        }

        if (!DeviceInfo.Current.Platform.Equals(DevicePlatform.Android))
        {
            _logger.LogDebug("Biometric unlock is only supported on Android");
            return true;
        }

        _logger.LogInformation("Biometric unlock is required, attempting authentication");

        try
        {
            var result = await _biometricService.AuthenticateAsync(
                new AuthenticationRequest(
                    title: "Unlock Good Diary",
                    reason: "Authenticate to access your journal")
                {
                    CancelTitle = "Cancel"
                });

            if (result.Status == AuthenticationStatus.Success)
            {
                _logger.LogInformation("Biometric authentication successful");
                return true;
            }
            else
            {
                _logger.LogWarning("Biometric authentication failed with status: {Status}", result.Status);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during biometric authentication");
            return false;
        }
    }
}
