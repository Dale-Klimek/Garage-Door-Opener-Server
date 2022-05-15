namespace GarageDoorOpener.Server.Interfaces;

internal interface IRightDoorPin
{
    public Task SendSignal(CancellationToken cancellationToken = default);
}

