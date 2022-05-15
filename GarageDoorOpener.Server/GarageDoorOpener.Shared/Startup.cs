using GarageDoorOpener.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GarageDoorOpener.Shared;

public static class Startup
{
    public static IServiceCollection AddGarageDoorOpenerShared(this IServiceCollection services)
    {
        services.AddSingleton<ITwoFactorAuthenticator, TwoFactorAuthenticator>();
        return services;
    }
}
