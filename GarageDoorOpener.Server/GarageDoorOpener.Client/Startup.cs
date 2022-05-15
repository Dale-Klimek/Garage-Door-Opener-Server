using GarageDoorOpener.Client.Interfaces;
using GarageDoorOpener.Client.Services;
using GarageDoorOpener.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace GarageDoorOpener.Client;

public static class Startup
{
    public static IServiceCollection AddGarageDoorOpenerClient(this IServiceCollection services)
    {
        services.AddGarageDoorOpenerShared();
        services.AddSingleton<IAuthenticatorService, AuthenticatorService>();
        return services;
    }
}