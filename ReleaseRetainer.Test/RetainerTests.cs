using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;
using ReleaseRetainer.Test.Builders;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Test;

[TestFixture]
public class RetainerTests
{
    private RetainerService _systemUnderTest;
    private MockLogger<RetainerService> _logger;
    private DeploymentTestBuilder _deploymentBuilder;
    private EnvironmentTestBuilder _environmentBuilder;
    private ProjectTestBuilder _projectBuilder;
    private ReleaseTestBuilder _releaseBuilder;
    private RetainReleaseOptionsTestBuilder _retainReleaseOptionsBuilder;
    private readonly DateTime _utcNow = DateTime.UtcNow;

    [SetUp]
    public void SetUp()
    {
        _retainReleaseOptionsBuilder = new RetainReleaseOptionsTestBuilder();
        _deploymentBuilder = new DeploymentTestBuilder();
        _environmentBuilder = new EnvironmentTestBuilder();
        _projectBuilder = new ProjectTestBuilder();
        _releaseBuilder = new ReleaseTestBuilder();
        _logger = Substitute.For<MockLogger<RetainerService>>();
        _systemUnderTest = new RetainerService(_logger);
    }

    [Test]
    [TestCase(-1, TestName = "NumOfReleasesToKeepIsLessThanZero")]
    [TestCase(0, TestName = "NumOfReleasesToKeepIsZero")]
    public void RetainReleases_ThrowsArgumentException_WhenNumOfReleasesToKeepIsLessOrEqualsToZero(int numOfReleasesToKeep)
    {
        // Arrange
        const string expectedExceptionMessage = $@"{nameof(RetainReleaseOptions.NumOfReleasesToKeep)} must be greater than zero. (Parameter '{nameof(RetainReleaseOptions.NumOfReleasesToKeep)}')";
        var options = _retainReleaseOptionsBuilder.WithNumOfReleasesToKeep(numOfReleasesToKeep).Build();
    
        // Act
        // Assert
        var exception = Assert.Throws<ArgumentException>(() => _systemUnderTest.RetainReleases(options));
    
        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Test]
    public void RetainReleases_KeepsReleaseWithSameDeploymentTime_ToTheSameEnvironment()
    {
        // Arrange
        var project = _projectBuilder
                      .WithId("Project-1")
                      .WithName("Random Quotes")
                      .Build();

        var release1 = _releaseBuilder
                       .WithId("Release-1")
                       .WithProjectId(project.Id)
                       .WithVersion("1.0.0")
                       .WithCreated(_utcNow)
                       .Build();
        var release2 = _releaseBuilder
                       .WithId("Release-2")
                       .WithProjectId(project.Id)
                       .WithVersion("1.0.1")
                       .WithCreated(_utcNow.AddHours(1))
                       .Build();

        var environment = _environmentBuilder
                          .WithId("Environment-1")
                          .WithName("Staging")
                          .Build();

        var deployment1 = _deploymentBuilder
                          .WithId("Deployment-1")
                          .WithReleaseId(release2.Id)
                          .WithEnvironmentId(environment.Id)
                          .WithDeployedAt(_utcNow)
                          .Build();
        var deployment2 = _deploymentBuilder
                          .WithId("Deployment-2")
                          .WithReleaseId(release1.Id)
                          .WithEnvironmentId(environment.Id)
                          .WithDeployedAt(_utcNow)
                          .Build();

        var deployments = new List<Deployment>
        {
            deployment1,
            deployment2
        };

        var environments = new List<Environment>
        {
            environment
        };

        var releases = new List<Release>
        {
            release1,
            release2
        };

        var projects = new List<Project>
        {
            _projectBuilder
                .WithId("Project-1")
                .WithName("Random Quotes")
                .Build()
        };

        var options = new RetainReleaseOptions
        {
            Deployments = deployments,
            Environments = environments,
            Projects = projects,
            Releases = releases,
            NumOfReleasesToKeep = 1
        };

        // Act
        _systemUnderTest.RetainReleases(options);

        // Assert
        _logger.Logs.Count.Should().Be(1);
        _logger.Logs.Should().Contain("'Release-2' kept because it was most recently deployed to 'Environment-1'");
    }

    [Test]
    public void RetainReleases_KeepsReleaseWithDifferentDeploymentTime_ToTheSameEnvironment()
    {
        // Arrange
        var project = _projectBuilder
                      .WithId("Project-1")
                      .WithName("Random Quotes")
                      .Build();

        var release1 = _releaseBuilder
                       .WithId("Release-1")
                       .WithProjectId(project.Id)
                       .WithVersion("1.0.0")
                       .WithCreated(_utcNow)
                       .Build();
        var release2 = _releaseBuilder
                       .WithId("Release-2")
                       .WithProjectId(project.Id)
                       .WithVersion("1.0.1")
                       .WithCreated(_utcNow.AddHours(1))
                       .Build();

        var environment = _environmentBuilder
                          .WithId("Environment-1")
                          .WithName("Staging")
                          .Build();

        var deployment1 = _deploymentBuilder
                          .WithId("Deployment-1")
                          .WithReleaseId(release2.Id)
                          .WithEnvironmentId(environment.Id)
                          .WithDeployedAt(_utcNow)
                          .Build();
        var deployment2 = _deploymentBuilder
                          .WithId("Deployment-2")
                          .WithReleaseId(release1.Id)
                          .WithEnvironmentId(environment.Id)
                          .WithDeployedAt(_utcNow.AddHours(1))
                          .Build();

        var deployments = new List<Deployment>
        {
            deployment1,
            deployment2
        };

        var environments = new List<Environment>
        {
            environment
        };

        var releases = new List<Release>
        {
            release1,
            release2
        };

        var projects = new List<Project>
        {
            _projectBuilder
                .WithId("Project-1")
                .WithName("Random Quotes")
                .Build()
        };

        var options = new RetainReleaseOptions
        {
            Deployments = deployments,
            Environments = environments,
            Projects = projects,
            Releases = releases,
            NumOfReleasesToKeep = 1
        };

        // Act
        _systemUnderTest.RetainReleases(options);

        // Assert
        _logger.LogInformation("'{ReleaseId}' kept because it was most recently deployed to '{EnvironmentId}'", "Release-1", "Environment-1");
    }

    [Test]
    public void RetainReleases_KeepsReleasesWithSameId_ForDifferentProjectAndEnvironmentCombinations()
    {
        // Arrange
        var project = _projectBuilder
                      .WithId("Project-1")
                      .WithName("Random Quotes")
                      .Build();

        var release1 = _releaseBuilder
                       .WithId("Release-1")
                       .WithProjectId(project.Id)
                       .WithVersion("1.0.0")
                       .WithCreated(_utcNow)
                       .Build();

        var environment1 = _environmentBuilder
                          .WithId("Environment-1")
                          .WithName("Staging")
                          .Build();
        var environment2 = _environmentBuilder
                           .WithId("Environment-2")
                           .WithName("Production")
                           .Build();

        var deployment1 = _deploymentBuilder
                          .WithId("Deployment-1")
                          .WithReleaseId(release1.Id)
                          .WithEnvironmentId(environment1.Id)
                          .WithDeployedAt(_utcNow)
                          .Build();
        var deployment2 = _deploymentBuilder
                          .WithId("Deployment-2")
                          .WithReleaseId(release1.Id)
                          .WithEnvironmentId(environment1.Id)
                          .WithDeployedAt(_utcNow.AddHours(1))
                          .Build();
        var deployment3 = _deploymentBuilder
                          .WithId("Deployment-3")
                          .WithReleaseId(release1.Id)
                          .WithEnvironmentId(environment2.Id)
                          .WithDeployedAt(_utcNow)
                          .Build();
        var deployment4 = _deploymentBuilder
                          .WithId("Deployment-4")
                          .WithReleaseId(release1.Id)
                          .WithEnvironmentId(environment2.Id)
                          .WithDeployedAt(_utcNow.AddHours(1))
                          .Build();

        var deployments = new List<Deployment>
        {
            deployment1,
            deployment2,
            deployment4,
            deployment3
        };

        var environments = new List<Environment>
        {
            environment1,
            environment2
        };

        var releases = new List<Release>
        {
            release1
        };

        var projects = new List<Project>
        {
            project
        };

        var options = new RetainReleaseOptions
        {
            Deployments = deployments,
            Environments = environments,
            Projects = projects,
            Releases = releases,
            NumOfReleasesToKeep = 1
        };

        // Act
        _systemUnderTest.RetainReleases(options);

        // Assert
        _logger.Logs.Count.Should().Be(2);
        _logger.Logs.Should().Contain("'Release-1' kept because it was most recently deployed to 'Environment-1'");
        _logger.Logs.Should().Contain("'Release-1' kept because it was most recently deployed to 'Environment-2'");
    }
}