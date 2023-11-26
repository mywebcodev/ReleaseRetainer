using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ReleaseRetainer.Models;

namespace ReleaseRetainer.Test.IntegrationTests;

[TestFixture]
public class RetainerServiceTests
{
    private ServiceProvider _serviceProvider;
    private IRetainerService _systemUnderTest;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _serviceProvider = new TestServiceContext().ServiceProvider;
        _systemUnderTest = _serviceProvider.GetService<IRetainerService>();
    }

    private async Task<RetainReleaseOptions> LoadData()
    {
        var projectTask = TestDataLoader.LoadProjectsAsync();
        var releasesTask = TestDataLoader.LoadReleasesAsync();
        var environmentsTask = TestDataLoader.LoadEnvironmentsAsync();
        var deploymentsTask = TestDataLoader.LoadDeploymentsAsync();

        await Task.WhenAll(projectTask, releasesTask, environmentsTask, deploymentsTask);

        return new RetainReleaseOptions
        {
            Deployments = await deploymentsTask,
            Environments = await environmentsTask,
            Projects = await projectTask,
            Releases = await releasesTask,
            NumOfReleasesToKeep = 2
        };
    }
}