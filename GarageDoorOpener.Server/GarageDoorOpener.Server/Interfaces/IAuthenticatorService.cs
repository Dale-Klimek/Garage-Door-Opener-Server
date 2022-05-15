namespace GarageDoorOpener.Server.Interfaces;

public interface IAuthenticatorService
{
    bool Authenticate(string inputCode);
}