using System;
using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using Microsoft.Extensions.Logging;
using RedGaint.Network.GameSessionModule;

public class ModuleConfig : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        config.Dependencies.AddSingleton(GameApiClient.Create());
        config.Dependencies.AddSingleton<IPushClient, PushClient>(_ => PushClient.Create());
        config.Dependencies.AddSingleton(new Random());
        config.Dependencies.AddSingleton<ILogger<GameSession>>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger<GameSession>();
        });
    }
}