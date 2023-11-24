using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ReleaseRetainer.Entities;
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

    // [Test]
    // [TestCase(-1, TestName = "NumOfReleasesToKeepIsLessThanZero")]
    // [TestCase(0, TestName = "NumOfReleasesToKeepIsZero")]
    // public void RetainReleases_ThrowsArgumentException_WhenNumOfReleasesToKeepIsLessOrEqualsToZero(int numOfReleasesToKeep)
    // {
    //     // Arrange
    //     const string expectedExceptionMessage = $@"{nameof(RetainReleaseOptions.NumOfReleasesToKeep)} must be greater than zero. (Parameter '{nameof(RetainReleaseOptions.NumOfReleasesToKeep)}')";
    //     var options = _retainReleaseOptionsBuilder.With(p => p.NumOfReleasesToKeep, numOfReleasesToKeep).Build();
    //
    //     // Act
    //     // Assert
    //     var exception = Assert.Throws<ArgumentException>(() => _systemUnderTest.RetainReleases(options));
    //
    //     exception.Message.Should().Be(expectedExceptionMessage);
    // }

    private void AssertLogMessage(string releaseId, string envId)
    {
        _logger.Logs.Should().Contain($"'{releaseId}' kept because it was most recently deployed to '{envId}'");
    }

    [Test]
    public void RetainReleases_KeepsReleaseWithSameDeploymentTime_ToTheSameEnvironment()
    {
        // Arrange
        var project = _projectBuilder.CreateRandom().Build();

        var release1 = _releaseBuilder.CreateRandom()
                                      .With(p => p.ProjectId, project.Id)
                                      .With(p => p.Created, _utcNow)
                                      .Build();
        var release2 = _releaseBuilder.CreateRandom()
                                      .With(p => p.ProjectId, project.Id)
                                      .With(p => p.Created, _utcNow.AddHours(1))
                                      .Build();
        var environment = _environmentBuilder.CreateRandom().Build();

        var deployment1 = _deploymentBuilder
                          .With(p => p.Id, "Deployment-1")
                          .With(p => p.ReleaseId, release2.Id)
                          .With(p => p.EnvironmentId, environment.Id)
                          .With(p => p.DeployedAt, _utcNow)
                          .Build();
        var deployment2 = _deploymentBuilder
                          .With(p => p.Id, "Deployment-2")
                          .With(p => p.ReleaseId, release1.Id)
                          .With(p => p.EnvironmentId, environment.Id)
                          .With(p => p.DeployedAt, _utcNow)
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
            project
        };

        var options = _retainReleaseOptionsBuilder
                      .With(p => p.Deployments, deployments)
                      .With(p => p.Environments, environments)
                      .With(p => p.Projects, projects)
                      .With(p => p.Releases, releases)
                      .With(p => p.NumOfReleasesToKeep, 1)
                      .Build();

        // Act
        _systemUnderTest.RetainReleases(options);

        // Assert
        _logger.Logs.Count.Should().Be(1);
        AssertLogMessage(release2.Id, environment.Id);
    }

    [Test]
    public void RetainReleases_KeepsReleaseWithDifferentDeploymentTime_ToTheSameEnvironment()
    {
        // Arrange
        var project = _projectBuilder.CreateRandom().Build();

        var release1 = _releaseBuilder
                       .CreateRandom()
                       .With(p => p.ProjectId, project.Id)
                       .With(p => p.Created, _utcNow)
                       .Build();
        var release2 = _releaseBuilder
                       .CreateRandom()
                       .With(p => p.ProjectId, project.Id)
                       .With(p => p.Created, _utcNow.AddHours(1))
                       .Build();

        var environment = _environmentBuilder.CreateRandom().Build();

        var deployment1 = _deploymentBuilder
                          .With(p => p.Id, "Deployment-1")
                          .With(p => p.ReleaseId, release2.Id)
                          .With(p => p.EnvironmentId, environment.Id)
                          .With(p => p.DeployedAt, _utcNow)
                          .Build();
        var deployment2 = _deploymentBuilder
                          .With(p => p.Id, "Deployment-2")
                          .With(p => p.ReleaseId, release1.Id)
                          .With(p => p.EnvironmentId, environment.Id)
                          .With(p => p.DeployedAt, _utcNow.AddHours(1))
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
            project
        };

        var options = _retainReleaseOptionsBuilder
                      .With(p => p.Deployments, deployments)
                      .With(p => p.Environments, environments)
                      .With(p => p.Projects, projects)
                      .With(p => p.Releases, releases)
                      .With(p => p.NumOfReleasesToKeep, 1)
                      .Build();

        // Act
        _systemUnderTest.RetainReleases(options);

        // Assert
        _logger.Logs.Count.Should().Be(1);
        AssertLogMessage(release1.Id, environment.Id);
    }

    [Test]
    public void RetainReleases_KeepsReleasesWithSameId_ForDifferentProjectAndEnvironmentCombinations()
    {
        // Arrange
        var project = _projectBuilder.CreateRandom().Build();

        var release = _releaseBuilder
                      .CreateRandom()
                      .With(p => p.ProjectId, project.Id)
                      .Build();

        var environment1 = _environmentBuilder
                           .CreateRandom()
                           .Build();
        var environment2 = _environmentBuilder
                           .CreateRandom()
                           .Build();

        var deployment1 = _deploymentBuilder
                          .With(p => p.Id, "Deployment-1")
                          .With(p => p.ReleaseId, release.Id)
                          .With(p => p.EnvironmentId, environment1.Id)
                          .With(p => p.DeployedAt, _utcNow)
                          .Build();
        var deployment2 = _deploymentBuilder
                          .With(p => p.Id, "Deployment-2")
                          .With(p => p.ReleaseId, release.Id)
                          .With(p => p.EnvironmentId, environment1.Id)
                          .With(p => p.DeployedAt, _utcNow.AddHours(1))
                          .Build();
        var deployment3 = _deploymentBuilder
                          .With(p => p.Id, "Deployment-3")
                          .With(p => p.ReleaseId, release.Id)
                          .With(p => p.EnvironmentId, environment2.Id)
                          .With(p => p.DeployedAt, _utcNow)
                          .Build();
        var deployment4 = _deploymentBuilder
                          .With(p => p.Id, "Deployment-4")
                          .With(p => p.ReleaseId, release.Id)
                          .With(p => p.EnvironmentId, environment2.Id)
                          .With(p => p.DeployedAt, _utcNow.AddHours(1))
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
            release
        };

        var projects = new List<Project>
        {
            project
        };

        var options = _retainReleaseOptionsBuilder
                      .With(p => p.Deployments, deployments)
                      .With(p => p.Environments, environments)
                      .With(p => p.Projects, projects)
                      .With(p => p.Releases, releases)
                      .With(p => p.NumOfReleasesToKeep, 1)
                      .Build();

        // Act
        _systemUnderTest.RetainReleases(options);

        // Assert
        _logger.Logs.Count.Should().Be(2);
        AssertLogMessage(release.Id, environment1.Id);
        AssertLogMessage(release.Id, environment2.Id);
    }
}