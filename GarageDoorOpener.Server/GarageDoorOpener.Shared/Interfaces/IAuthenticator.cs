namespace GarageDoorOpener.Shared.Interfaces;

public interface IAuthenticator
{
    string GetKey();
    bool IsAuthenticated(string key);
}
