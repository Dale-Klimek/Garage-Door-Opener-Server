namespace GarageDoorOpener.Client.Interfaces;

public interface IAuthenticatorService
{
    string GetKey(string secretKey);
}