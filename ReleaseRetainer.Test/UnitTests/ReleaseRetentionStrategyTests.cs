using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;
using ReleaseRetainer.Strategies;
using ReleaseRetainer.Test.Builders;
using ReleaseRetainer.Test.Mocks;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Test.UnitTests;

[TestFixture]
public class ReleaseRetentionStrategyTests
{
    private ReleaseRetentionStrategy _systemUnderTest;
    private MockLogger<ReleaseRetentionStrategy> _logger;
    private static readonly DeploymentBuilder DeploymentBuilder = new();
    private static readonly EnvironmentBuilder EnvironmentBuilder = new();
    private static readonly ProjectBuilder ProjectBuilder = new();
    private static readonly ReleaseBuilder ReleaseBuilder = new();
    private static readonly RetainReleaseOptionsBuilder RetainReleaseOptionsBuilder = new();
    private static readonly DateTime UtcNow = DateTime.UtcNow;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<MockLogger<ReleaseRetentionStrategy>>();
        _systemUnderTest = new ReleaseRetentionStrategy(_logger);
    }

    private void AssertLogMessage(string releaseId, string envId)
    {
        _logger.Logs.Should().Contain($"'{releaseId}' kept because it was most recently deployed to '{envId}'");
    }

    private static IEnumerable<TestCaseData> EmptyCollectionsTestCases()
    {
        var project = ProjectBuilder.CreateRandom().Build();

        var release = ReleaseBuilder
                      .CreateRandom()
                      .With(p => p.ProjectId, project.Id)
                      .Build();
        var environment = EnvironmentBuilder
                          .CreateRandom()
                          .Build();

        var deployment = DeploymentBuilder
                         .With(p => p.Id, "Deployment-1")
                         .With(p => p.ReleaseId, release.Id)
                         .With(p => p.EnvironmentId, environment.Id)
                         .With(p => p.DeployedAt, UtcNow)
                         .Build();

        var deployments = new List<Deployment>
        {
            deployment
        };

        var environments = new List<Environment>
        {
            environment
        };

        var releases = new List<Release>
        {
            release
        };

        var projects = new List<Project>
        {
            project
        };

        yield return new TestCaseData(RetainReleaseOptionsBuilder
                                      .With(p => p.Deployments, Array.Empty<Deployment>())
                                      .With(p => p.Environments, environments)
                                      .With(p => p.Projects, projects)
                                      .With(p => p.Releases, releases)
                                      .With(p => p.NumOfReleasesToKeep, 1)
                                      .Build())
        { TestName = "Deployments" };
        yield return new TestCaseData(RetainReleaseOptionsBuilder
                                      .With(p => p.Deployments, deployments)
                                      .With(p => p.Environments, Array.Empty<Environment>())
                                      .With(p => p.Projects, projects)
                                      .With(p => p.Releases, releases)
                                      .With(p => p.NumOfReleasesToKeep, 1)
                                      .Build())
        { TestName = "Environments" };
        yield return new TestCaseData(RetainReleaseOptionsBuilder
                                      .With(p => p.Deployments, deployments)
                                      .With(p => p.Environments, environments)
                                      .With(p => p.Projects, Array.Empty<Project>())
                                      .With(p => p.Releases, releases)
                                      .With(p => p.NumOfReleasesToKeep, 1)
                                      .Build())
        { TestName = "Projects" };
        yield return new TestCaseData(RetainReleaseOptionsBuilder
                                      .With(p => p.Deployments, deployments)
                                      .With(p => p.Environments, environments)
                                      .With(p => p.Projects, projects)
                                      .With(p => p.Releases, Array.Empty<Release>())
                                      .With(p => p.NumOfReleasesToKeep, 1)
                                      .Build())
        { TestName = "Release" };
    }

    private static IEnumerable<TestCaseData> OrphanedReleasesTestCases()
    {
        var project = ProjectBuilder.CreateRandom().Build();

        var release = ReleaseBuilder
                      .CreateRandom()
                      .With(p => p.ProjectId, project.Id)
                      .Build();
        var environment = EnvironmentBuilder
                          .CreateRandom()
                          .Build();

        var environments = new List<Environment>
        {
            environment
        };

        var releases = new List<Release>
        {
            release
        };

        var projects = new List<Project>
        {
            project
        };

        var releaseWithoutProject = ReleaseBuilder.CreateRandom()
                                                  .With(p => p.ProjectId, null)
                                                  .Build();

        var deploymentForReleaseWithoutProject = DeploymentBuilder
                                             .CreateRandom()
                                             .With(p => p.ReleaseId, releaseWithoutProject.Id)
                                             .With(p => p.EnvironmentId, environment.Id)
                                             .Build();

        var deploymentWithDifferentRelease = DeploymentBuilder.CreateRandom().Build();

        yield return new TestCaseData(RetainReleaseOptionsBuilder
                                      .With(p => p.Deployments, new List<Deployment> { deploymentForReleaseWithoutProject })
                                      .With(p => p.Environments, environments)
                                      .With(p => p.Projects, projects)
                                      .With(p => p.Releases, new List<Release> { releaseWithoutProject })
                                      .With(p => p.NumOfReleasesToKeep, 1)
                                      .Build())
        { TestName = "ReleaseWithoutProject" };
        yield return new TestCaseData(RetainReleaseOptionsBuilder
                                      .With(p => p.Deployments, new List<Deployment> { deploymentWithDifferentRelease })
                                      .With(p => p.Environments, environments)
                                      .With(p => p.Projects, projects)
                                      .With(p => p.Releases, releases)
                                      .With(p => p.NumOfReleasesToKeep, 1)
                                      .Build())
        { TestName = "ReleaseWithoutDeployment" };
    }

    [Test]
    public void RetainReleases_KeepsReleaseWithSameDeploymentTime_ToTheSameEnvironment()
    {
        // Arrange
        var project = ProjectBuilder.CreateRandom().Build();

        var release1 = ReleaseBuilder.CreateRandom()
                                      .With(p => p.ProjectId, project.Id)
                                      .With(p => p.Created, UtcNow)
                                      .Build();
        var release2 = ReleaseBuilder.CreateRandom()
                                      .With(p => p.ProjectId, project.Id)
                                      .With(p => p.Created, UtcNow.AddHours(1))
                                      .Build();
        var environment = EnvironmentBuilder.CreateRandom().Build();

        var deployment1 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-1")
                          .With(p => p.ReleaseId, release2.Id)
                          .With(p => p.EnvironmentId, environment.Id)
                          .With(p => p.DeployedAt, UtcNow)
                          .Build();
        var deployment2 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-2")
                          .With(p => p.ReleaseId, release1.Id)
                          .With(p => p.EnvironmentId, environment.Id)
                          .With(p => p.DeployedAt, UtcNow)
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

        var options = RetainReleaseOptionsBuilder
                      .With(p => p.Deployments, deployments)
                      .With(p => p.Environments, environments)
                      .With(p => p.Projects, projects)
                      .With(p => p.Releases, releases)
                      .With(p => p.NumOfReleasesToKeep, 1)
                      .Build();

        var expectedResult = new[] { release2 };

        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Logs.Count.Should().Be(1);
        AssertLogMessage(release2.Id, environment.Id);
    }

    [Test]
    public void RetainReleases_KeepsReleaseWithDifferentDeploymentTime_ToTheSameEnvironment()
    {
        // Arrange
        var project = ProjectBuilder.CreateRandom().Build();

        var release1 = ReleaseBuilder
                       .CreateRandom()
                       .With(p => p.ProjectId, project.Id)
                       .With(p => p.Created, UtcNow)
                       .Build();
        var release2 = ReleaseBuilder
                       .CreateRandom()
                       .With(p => p.ProjectId, project.Id)
                       .With(p => p.Created, UtcNow.AddHours(1))
                       .Build();

        var environment = EnvironmentBuilder.CreateRandom().Build();

        var deployment1 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-1")
                          .With(p => p.ReleaseId, release2.Id)
                          .With(p => p.EnvironmentId, environment.Id)
                          .With(p => p.DeployedAt, UtcNow)
                          .Build();
        var deployment2 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-2")
                          .With(p => p.ReleaseId, release1.Id)
                          .With(p => p.EnvironmentId, environment.Id)
                          .With(p => p.DeployedAt, UtcNow.AddHours(1))
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

        var options = RetainReleaseOptionsBuilder
                      .With(p => p.Deployments, deployments)
                      .With(p => p.Environments, environments)
                      .With(p => p.Projects, projects)
                      .With(p => p.Releases, releases)
                      .With(p => p.NumOfReleasesToKeep, 1)
                      .Build();

        var expectedResult = new[] { release1 };

        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Logs.Count.Should().Be(1);
        AssertLogMessage(release1.Id, environment.Id);
    }

    [Test]
    public void KeepsReleasesWithSameId_ForDifferentProjectAndEnvironmentCombinations()
    {
        // Arrange
        var project = ProjectBuilder.CreateRandom().Build();

        var release = ReleaseBuilder
                      .CreateRandom()
                      .With(p => p.ProjectId, project.Id)
                      .Build();

        var environment1 = EnvironmentBuilder
                           .CreateRandom()
                           .Build();
        var environment2 = EnvironmentBuilder
                           .CreateRandom()
                           .Build();

        var deployment1 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-1")
                          .With(p => p.ReleaseId, release.Id)
                          .With(p => p.EnvironmentId, environment1.Id)
                          .With(p => p.DeployedAt, UtcNow)
                          .Build();
        var deployment2 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-2")
                          .With(p => p.ReleaseId, release.Id)
                          .With(p => p.EnvironmentId, environment1.Id)
                          .With(p => p.DeployedAt, UtcNow.AddHours(1))
                          .Build();
        var deployment3 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-3")
                          .With(p => p.ReleaseId, release.Id)
                          .With(p => p.EnvironmentId, environment2.Id)
                          .With(p => p.DeployedAt, UtcNow)
                          .Build();
        var deployment4 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-4")
                          .With(p => p.ReleaseId, release.Id)
                          .With(p => p.EnvironmentId, environment2.Id)
                          .With(p => p.DeployedAt, UtcNow.AddHours(1))
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

        var options = RetainReleaseOptionsBuilder
                      .With(p => p.Deployments, deployments)
                      .With(p => p.Environments, environments)
                      .With(p => p.Projects, projects)
                      .With(p => p.Releases, releases)
                      .With(p => p.NumOfReleasesToKeep, 1)
                      .Build();

        var expectedResult = new[] { release, release };

        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Logs.Count.Should().Be(2);
        AssertLogMessage(release.Id, environment1.Id);
        AssertLogMessage(release.Id, environment2.Id);
    }


    [Test]
    [TestCaseSource(nameof(EmptyCollectionsTestCases))]
    public void ShouldNotThrowException_WhenCollectionsAreEmpty(RetainReleaseOptions options)
    {
        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEmpty();
        _logger.Logs.Count.Should().Be(0);
    }

    [Test]
    [TestCaseSource(nameof(OrphanedReleasesTestCases))]
    public void ShouldNotThrowException_WhenReleaseIsOrphaned(RetainReleaseOptions options)
    {
        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEmpty();
        _logger.Logs.Count.Should().Be(0);
    }

    [Test]
    public void ShouldNotThrowException_WhenDeploymentWithoutEnvironment()
    {
        // Arrange
        var project = ProjectBuilder.CreateRandom().Build();

        var release = ReleaseBuilder
                      .CreateRandom()
                      .With(p => p.ProjectId, project.Id)
                      .Build();

        var environment = EnvironmentBuilder
                           .CreateRandom()
                           .Build();

        var deployment1 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-1")
                          .With(p => p.ReleaseId, release.Id)
                          .With(p => p.EnvironmentId, null)
                          .With(p => p.DeployedAt, UtcNow)
                          .Build();

        var deployments = new List<Deployment>
        {
            deployment1
        };

        var environments = new List<Environment>
        {
            environment
        };

        var releases = new List<Release>
        {
            release
        };

        var projects = new List<Project>
        {
            project
        };

        var options = RetainReleaseOptionsBuilder
                      .With(p => p.Deployments, deployments)
                      .With(p => p.Environments, environments)
                      .With(p => p.Projects, projects)
                      .With(p => p.Releases, releases)
                      .With(p => p.NumOfReleasesToKeep, 5)
                      .Build();

        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEmpty();
        _logger.Logs.Count.Should().Be(0);
    }

    [Test]
    public void ShouldRetainAllReleasesInProjectAndEnvironment_WhenProjectsWithFewerReleasesThanNumOfReleasesToKeep()
    {
        // Arrange
        var project = ProjectBuilder.CreateRandom().Build();

        var release1 = ReleaseBuilder
                      .CreateRandom()
                      .With(p => p.ProjectId, project.Id)
                      .Build();

        var release2 = ReleaseBuilder
                      .CreateRandom()
                      .With(p => p.ProjectId, project.Id)
                      .Build();

        var environment = EnvironmentBuilder
                           .CreateRandom()
                           .Build();

        var deployment1 = DeploymentBuilder
                          .With(p => p.Id, "Deployment-1")
                          .With(p => p.ReleaseId, release1.Id)
                          .With(p => p.EnvironmentId, environment.Id)
                          .With(p => p.DeployedAt, UtcNow)
                          .Build();

        var deployment2 = DeploymentBuilder
                         .With(p => p.Id, "Deployment-2")
                         .With(p => p.ReleaseId, release2.Id)
                         .With(p => p.EnvironmentId, environment.Id)
                         .With(p => p.DeployedAt, UtcNow)
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

        var options = RetainReleaseOptionsBuilder
                      .With(p => p.Deployments, deployments)
                      .With(p => p.Environments, environments)
                      .With(p => p.Projects, projects)
                      .With(p => p.Releases, releases)
                      .With(p => p.NumOfReleasesToKeep, 5)
                      .Build();

        var expectedResult = new[] { release1, release2 };

        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Logs.Count.Should().Be(2);
        AssertLogMessage(release1.Id, environment.Id);
        AssertLogMessage(release2.Id, environment.Id);
    }
}