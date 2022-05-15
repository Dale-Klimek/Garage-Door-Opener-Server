using GarageDoorOpener.Server;
using GarageDoorOpener.Server.Services;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using System.Device.Gpio;
using GarageDoorOpener.Server.Controllers;
using Serilog.Sinks.SystemConsole.Themes;
using GarageDoorOpener.Shared;
using GarageDoorOpener.Shared.Interfaces;
using GarageDoorOpener.Server.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProcessName()
    .Enrich.WithThreadName()
    .MinimumLevel.Information()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, configuration) => 
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .Enrich.WithProcessName()
        .Enrich.WithThreadName()
        .WriteTo.Console(theme: AnsiConsoleTheme.Code));

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<IGarageDoorController, GarageDoorController>();
builder.Services.AddSingleton<GpioController>();
builder.Services.AddSingleton<IRightDoorPin, PinController>(p =>
{
    var config = p.GetRequiredService<IOptions<Configuration>>().Value;
    var controller = p.GetRequiredService<GpioController>();
    
    return new PinController(p.GetRequiredService<ILogger<PinController>>(), controller, config.RightDoorPin, config.Delay);
});
builder.Services.AddSingleton<ILeftDoorPin, PinController>(p =>
{
    var controller = p.GetRequiredService<GpioController>();
    var config = p.GetRequiredService<IOptions<Configuration>>().Value;
    return new PinController(p.GetRequiredService<ILogger<PinController>>(), controller, config.LeftDoorPin, config.Delay);
});

builder.Services.Configure<Configuration>(builder.Configuration.GetSection(Configuration.Config));

builder.Services.AddGarageDoorOpenerShared();
builder.Services.AddSingleton<IAuthenticatorService, AuthenticatorService>(p =>
    new AuthenticatorService(p.GetRequiredService<ITwoFactorAuthenticator>(), builder.Configuration["SharedSecret"]));
builder.Services.AddGrpcReflection();

try
{
    Log.Verbose("Starting web server");
    var app = builder.Build();

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<GarageDoorService>();
    if (app.Environment.IsDevelopment())
        app.MapGrpcReflectionService();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}