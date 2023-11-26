using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

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

    private void LoadData()
    {
        var projectTask = TestDataLoader.LoadProjectsAsync();
        var releasesTask = TestDataLoader.LoadReleasesAsync();
        var environmentsTask = TestDataLoader.LoadEnvironmentsAsync();
        var deploymentsTask = TestDataLoader.LoadDeploymentsAsync();
    }
}