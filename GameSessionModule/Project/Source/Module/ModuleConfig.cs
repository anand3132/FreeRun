using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using RedGaint.Network.GameSessionModule;

/// <summary>
/// ModuleConfig is the entry point for setting up Dependency Injection (DI)
/// in Unity Cloud Code. It registers all required services, clients, and loggers
/// for the multiplayer game session system.
/// </summary>
public class ModuleConfig : ICloudCodeSetup
{
    /// <summary>
    /// Registers all dependencies needed by the Cloud Code module.
    /// This method is automatically called by Unity Cloud Code during deployment.
    /// </summary>
    /// <param name="config">The DI configuration container provided by Cloud Code.</param>
    public void Setup(ICloudCodeConfig config)
    {
        // ------------------------------------
        // Unity Service API Clients
        // ------------------------------------
        
        // config.Dependencies.AddHttpClient();


        // Lobby and matchmaking client for managing game lobbies
        config.Dependencies.AddSingleton(GameApiClient.Create());

        // Push client for real-time messaging or event-driven updates
        config.Dependencies.AddSingleton<IPushClient, PushClient>(_ => PushClient.Create());

        // ------------------------------------
        // Shared Utility Services
        // ------------------------------------

        // System-level random number generator used across services
        config.Dependencies.AddSingleton(new Random());

        // ------------------------------------
        // Game Session Module Services
        // ------------------------------------

        // Core services that encapsulate game session and lobby logic
        config.Dependencies.AddSingleton<LobbyService>();
        config.Dependencies.AddSingleton<LobbyMonitorService>();
        config.Dependencies.AddSingleton<DedicatedServerService>();
        config.Dependencies.AddSingleton<PlayerDataBuilder>();
        

        // ------------------------------------
        // Logging Services
        // ------------------------------------

        // Logger specifically for GameSession class
        config.Dependencies.AddSingleton<ILogger<GameSession>>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger<GameSession>();
        });

        // Optional loggers for modular services — useful for debugging and observability
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
