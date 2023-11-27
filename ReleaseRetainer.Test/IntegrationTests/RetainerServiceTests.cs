using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;
using ReleaseRetainer.Strategies;
using ReleaseRetainer.Test.Builders;
using ReleaseRetainer.Test.Mocks;

namespace ReleaseRetainer.Test.IntegrationTests;

[TestFixture]
public class RetainerServiceTests
{
    private static readonly ReleaseRetainOptionsBuilder ReleaseRetainOptionsBuilder = new();
    private ServiceProvider _serviceProvider;
    private IRetainerService _systemUnderTest;
    private MockLogger<ReleaseRetentionStrategy> _logger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _serviceProvider = new TestServiceContext().ServiceProvider;
    }

    [SetUp]
    public void SetUp()
    {
        _logger = (MockLogger<ReleaseRetentionStrategy>) _serviceProvider.GetService<ILogger<ReleaseRetentionStrategy>>();
        _systemUnderTest = _serviceProvider.GetService<IRetainerService>();
    }

    private static async Task<ReleaseRetainOptions> CreateRetainReleaseOptions(int numOfReleasesToKeep)
    {
        var projectTask = TestDataLoader.LoadProjectsAsync();
        var releasesTask = TestDataLoader.LoadReleasesAsync();
        var environmentsTask = TestDataLoader.LoadEnvironmentsAsync();
        var deploymentsTask = TestDataLoader.LoadDeploymentsAsync();

        await Task.WhenAll(projectTask, releasesTask, environmentsTask, deploymentsTask);

        return ReleaseRetainOptionsBuilder
               .With(p => p.Deployments, await deploymentsTask)
               .With(p => p.Environments, await environmentsTask)
               .With(p => p.Projects, await projectTask)
               .With(p => p.Releases, await releasesTask)
               .With(p => p.NumOfReleasesToKeep, numOfReleasesToKeep)
               .Build();
    }

    private void AssertLogMessage(string releaseId, string envId)
    {
        _logger.Logs.Should().Contain(Expectations.LogExpectations.CreateExpectedRetainedReleaseLogMessage(releaseId, envId));
    }

    [Test]
    public async Task RetainReleases()
    {
        // Arrange
        var options = await CreateRetainReleaseOptions(2);
        var environment1 = options.Environments.First(e => e.Id == "Environment-1");
        var environment2 = options.Environments.First(e => e.Id == "Environment-2");

        var expectedRelease1 = options.Releases.First(r => r.Id == "Release-1");
        var expectedRelease2 = options.Releases.First(r => r.Id == "Release-2");
        var expectedRelease6 = options.Releases.First(r => r.Id == "Release-6");
        var expectedRelease7 = options.Releases.First(r => r.Id == "Release-7");

        var expectedRetainReleases = new List<Release>
        {
            expectedRelease1,
            expectedRelease1,
            expectedRelease2,
            expectedRelease6,
            expectedRelease6,
            expectedRelease7
        };

        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEquivalentTo(expectedRetainReleases);
        _logger.Logs.Count.Should().Be(expectedRetainReleases.Count);
        AssertLogMessage(expectedRelease1.Id, environment1.Id);
        AssertLogMessage(expectedRelease1.Id, environment2.Id);
        AssertLogMessage(expectedRelease2.Id, environment1.Id);
        AssertLogMessage(expectedRelease6.Id, environment1.Id);
        AssertLogMessage(expectedRelease6.Id, environment2.Id);
        AssertLogMessage(expectedRelease7.Id, environment1.Id);
    }
}