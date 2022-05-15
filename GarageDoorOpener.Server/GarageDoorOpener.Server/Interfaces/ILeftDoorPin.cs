namespace GarageDoorOpener.Server.Interfaces;

internal interface ILeftDoorPin
{
    public Task SendSignal(CancellationToken cancellationToken = default);
}

