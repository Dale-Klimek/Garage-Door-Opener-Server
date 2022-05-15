using GarageDoorOpener.Server.Interfaces;

namespace GarageDoorOpener.Server.Controllers;

//https://gunnarpeipman.com/aspnet-core-iot-led-switch/
//https://docs.microsoft.com/en-us/dotnet/iot/intro
//https://github.com/dotnet/iot/blob/main/Documentation/README.md
//https://docs.microsoft.com/en-us/dotnet/iot/tutorials/blink-led
//https://github.com/dotnet/iot/issues/764
internal class GarageDoorController : IGarageDoorController
{
    private readonly ILogger<GarageDoorController> _logger;
    private readonly IRightDoorPin _rightDoorPin;
    private readonly ILeftDoorPin _leftDoorPin;

    public GarageDoorController(ILogger<GarageDoorController> logger, IRightDoorPin rightDoorPin, ILeftDoorPin leftDoorPin)
    {
        _logger = logger;
        _rightDoorPin = rightDoorPin;
        _leftDoorPin = leftDoorPin;
    }

    public Task SignalRightDoor(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending signal to right door");
        return _rightDoorPin.SendSignal(cancellationToken);
    }

    public Task SignalLeftDoor(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending signal to left door");
        return _leftDoorPin.SendSignal(cancellationToken);
    }
}
