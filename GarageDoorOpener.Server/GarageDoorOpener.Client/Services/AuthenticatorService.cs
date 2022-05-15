using GarageDoorOpener.Client.Interfaces;
using GarageDoorOpener.Shared.Interfaces;

namespace GarageDoorOpener.Client.Services;

internal class AuthenticatorService : IAuthenticatorService
{
    private readonly ITwoFactorAuthenticator _authenticator;

    public AuthenticatorService(ITwoFactorAuthenticator authenticator)
    {
        _authenticator = authenticator;
    }

    public string GetKey(string secretKey)
    {
        return _authenticator.GetCurrentPIN(secretKey);
    }
}