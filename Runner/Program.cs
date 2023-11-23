using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using ReleaseRetainer;
using ReleaseRetainer.Models;

// Setting up dependency injection
var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);
var serviceProvider = serviceCollection.BuildServiceProvider();
var retainer = serviceProvider.GetService<IRetainerService>();

var projectTask = DataLoader.LoadProjectsAsync();
var releasesTask = DataLoader.LoadReleasesAsync();
var environmentsTask = DataLoader.LoadEnvironmentsAsync();
var deploymentsTask = DataLoader.LoadDeploymentsAsync();

await Task.WhenAll(projectTask, releasesTask, environmentsTask, deploymentsTask);

var result = retainer.RetainReleases(new RetainReleaseOptions
{
    Deployments = await deploymentsTask,
    Environments = await environmentsTask,
    Projects = await projectTask,
    Releases = await releasesTask,
    NumOfReleasesToKeep = 2
});
Console.WriteLine("Hello, World!");
return;

static void ConfigureServices(ServiceCollection services)
{
    services.AddLogging(config =>
    {
        config.AddDebug(); // Log to debug (debug window in Visual Studio or any debugger attached)
        config.AddConsole(); // Log to console
    }) .Configure<LoggerFilterOptions>(options =>
    {
        options.AddFilter<DebugLoggerProvider>(null /* category*/ , LogLevel.Information /* min level */);
        options.AddFilter<ConsoleLoggerProvider>(null  /* category*/ , LogLevel.Warning /* min level */);
    }).AddTransient<IRetainerService, RetainerService>();
}