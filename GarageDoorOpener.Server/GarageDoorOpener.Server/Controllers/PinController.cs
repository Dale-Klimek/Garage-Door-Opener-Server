using GarageDoorOpener.Server.Interfaces;
using Nito.AsyncEx;
using System.Device.Gpio;
using Timer = System.Timers.Timer;

namespace GarageDoorOpener.Server.Controllers;

internal class PinController : IDisposable, IRightDoorPin, ILeftDoorPin
{
    private readonly ILogger<PinController> _logger;
    private readonly GpioController _controller;
    private readonly Timer _timer;
    private readonly int _pinNumber;
    private bool _disposedValue;
    private readonly object _lock = new();
    private readonly AsyncLock _asyncLock = new();
    private readonly AsyncConditionVariable _notifier;


    public PinController(ILogger<PinController> logger, GpioController controller, int pinNumber, int timeDelay)
    {
        _logger = logger;
        _logger.LogTrace("Creating a pin controller for pin {pinNumber}", pinNumber);
        _controller = controller;
        _notifier = new(_asyncLock);
        _pinNumber = pinNumber;
        _controller.OpenPin(pinNumber, PinMode.Output, PinValue.High);
        _timer = new(timeDelay);
        _timer.Elapsed += TimerElapsed;
        _timer.AutoReset = false;
    }

    public async Task SendSignal(CancellationToken cancellationToken = default)
    {
        if (_controller.Read(_pinNumber) == PinValue.Low)
        {
            _logger.LogTrace("Waiting for last thread to complete");
            await _notifier.WaitAsync(cancellationToken);
        }
        _logger.LogTrace("Updating pin to low value {pinNumber}", _pinNumber);
        Write(PinValue.Low);
        _logger.LogTrace("Starting timer for pin: {pinNumber}", _pinNumber);
        _timer.Start();
    }

    private void TimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _timer.Stop();
        _logger.LogTrace("timer has exited");
        Write(PinValue.High);
        _logger.LogTrace("updated pin value to high for pin {pinNumber}", _pinNumber);
        _notifier.Notify();
        _logger.LogTrace("updated waiting threads to notify of being freed up");
    }

    private void Write(PinValue value)
    {
        lock (_lock)
        {
            _logger.LogTrace("Obtained lock on pin {pin}", _pinNumber);
            _controller.Write(_pinNumber, value);
            _logger.LogTrace("Wrote pin value: {pinvalue} to pin number {pinNumber}", value, _pinNumber);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _logger.LogTrace("Disposing pin controller");
                _timer.Elapsed -= TimerElapsed;
                _timer.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Test()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
