namespace GarageDoorOpener.Shared.Interfaces;

public interface ITwoFactorAuthenticator
{
    string GetCurrentPIN(string accountSecretKey);
    string[] GetCurrentPINs(string accountSecretKey);
    bool ValidateTwoFactorPIN(string accountSecretKey, string twoFactorCodeFromClient);
}