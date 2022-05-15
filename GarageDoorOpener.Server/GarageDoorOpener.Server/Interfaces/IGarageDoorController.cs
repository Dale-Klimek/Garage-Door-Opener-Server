namespace GarageDoorOpener.Server.Interfaces;

public interface IGarageDoorController
{
    Task SignalLeftDoor(CancellationToken cancellationToken = default);
    Task SignalRightDoor(CancellationToken cancellationToken = default);
}
