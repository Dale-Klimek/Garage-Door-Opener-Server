using GarageDoorOpener.Server.Interfaces;
using GarageDoorOpener.Shared.Interfaces;

namespace GarageDoorOpener.Server;

public class AuthenticatorService : IAuthenticatorService
{
    private readonly ITwoFactorAuthenticator _authenticator;
    private readonly string _secretKey;

    public AuthenticatorService(ITwoFactorAuthenticator authenticator, string secretKey)
    {
        _authenticator = authenticator;
        _secretKey = secretKey;
    }

    public bool Authenticate(string inputCode)
    {
        return _authenticator.ValidateTwoFactorPIN(_secretKey, inputCode);
    }
}