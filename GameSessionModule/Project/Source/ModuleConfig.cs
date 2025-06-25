using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using RedGaint.Network.GameSessionModule;

public class ModuleConfig : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        // Unity service clients
        config.Dependencies.AddSingleton(GameApiClient.Create());
        config.Dependencies.AddSingleton<IPushClient, PushClient>(_ => PushClient.Create());

        // Shared system utilities
        config.Dependencies.AddSingleton(new Random());

        // Services in GameSessionModule
        config.Dependencies.AddSingleton<LobbyService>();
        config.Dependencies.AddSingleton<LobbyMonitorService>();
        config.Dependencies.AddSingleton<DedicatedServerService>();
        config.Dependencies.AddSingleton<PlayerDataBuilder>();

        // Logger for GameSession
        config.Dependencies.AddSingleton<ILogger<GameSession>>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger<GameSession>();
        });

        // Logger for other services (optional but helpful)
        config.Dependencies.AddSingleton<ILogger<LobbyService>>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger<LobbyService>();
        });

        config.Dependencies.AddSingleton<ILogger<LobbyMonitorService>>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger<LobbyMonitorService>();
        });

        config.Dependencies.AddSingleton<ILogger<DedicatedServerService>>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger<DedicatedServerService>();
        });
    }
}