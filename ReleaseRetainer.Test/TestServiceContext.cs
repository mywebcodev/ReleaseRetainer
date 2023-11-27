using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using ReleaseRetainer.Strategies;
using ReleaseRetainer.Test.Mocks;

namespace ReleaseRetainer.Test;

public class TestServiceContext
{
    public ServiceProvider ServiceProvider { get; }

    public TestServiceContext()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddLogging(config =>
                {
                    config.AddDebug(); // Log to debug (debug window in Visual Studio or any debugger attached)
                    config.AddConsole(); // Log to console
                }).Configure<LoggerFilterOptions>(options =>
                {
                    options.AddFilter<DebugLoggerProvider>(null /* category*/, LogLevel.Information /* min level */);
                    options.AddFilter<ConsoleLoggerProvider>(null /* category*/, LogLevel.Warning /* min level */);
                });

        services.AddSingleton<ILogger<ReleaseRetentionStrategy>, MockLogger<ReleaseRetentionStrategy>>();
        services.AddTransient<IRetainerService, RetainerService>();
        services.AddTransient<IReleaseRetentionStrategy, ReleaseRetentionStrategy>();
    }
}