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
    private static readonly ReleaseRetainOptionsBuilder ReleaseRetainOptionsBuilder = new();
    private static readonly DateTime UtcNow = DateTime.UtcNow;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<MockLogger<ReleaseRetentionStrategy>>();
        _systemUnderTest = new ReleaseRetentionStrategy(_logger);
    }

    private void AssertLogMessage(string releaseId, string envId)
    {
        _logger.Logs.Should().Contain(Expectations.LogExpectations.CreateExpectedRetainedReleaseLogMessage(releaseId, envId));
    }

    private static Project CreateProject()
    {
        return ProjectBuilder.CreateRandom().Build();
    }

    private static Release CreateRelease(string projectId, DateTime created)
    {
        return ReleaseBuilder
               .CreateRandom()
               .With(p => p.ProjectId, projectId)
               .With(p => p.Created, created)
               .Build();
    }

    private static Environment CreateEnvironment()
    {
        return EnvironmentBuilder
               .CreateRandom()
               .Build();
    }

    private static ReleaseRetainOptions CreateReleaseRetainOptions(IEnumerable<Deployment> deployments, IEnumerable<Environment> environments, IEnumerable<Project> projects, IEnumerable<Release> releases, int numOfReleasesToKeep)
    {
        return ReleaseRetainOptionsBuilder
               .With(p => p.Deployments, deployments)
               .With(p => p.Environments, environments)
               .With(p => p.Projects, projects)
               .With(p => p.Releases, releases)
               .With(p => p.NumOfReleasesToKeep, numOfReleasesToKeep)
               .Build();
    }

    private static Deployment CreateDeployment(string releaseId, string environmentId, DateTime deployedAt)
    {
        return DeploymentBuilder
               .CreateRandom()
               .With(p => p.ReleaseId, releaseId)
               .With(p => p.EnvironmentId, environmentId)
               .With(p => p.DeployedAt, deployedAt)
               .Build();
    }

    private static IEnumerable<TestCaseData> EmptyCollectionsTestCases()
    {
        var project = ProjectBuilder.CreateRandom().Build();
        var release = CreateRelease(project.Id, UtcNow);
        var environment = CreateEnvironment();
        var deployment = CreateDeployment(release.Id, environment.Id, UtcNow);

        var deployments = new List<Deployment> {deployment};
        var environments = new List<Environment> {environment};
        var releases = new List<Release> {release};
        var projects = new List<Project> {project};
        var numOfReleasesToKeep = 5;

        yield return new TestCaseData(CreateReleaseRetainOptions(Array.Empty<Deployment>(), environments, projects, releases, numOfReleasesToKeep)) {TestName = "Deployments"};
        yield return new TestCaseData(CreateReleaseRetainOptions(deployments, Array.Empty<Environment>(), projects, releases, numOfReleasesToKeep)) {TestName = "Environments"};
        yield return new TestCaseData(CreateReleaseRetainOptions(deployments, environments, Array.Empty<Project>(), releases, numOfReleasesToKeep)) {TestName = "Projects"};
        yield return new TestCaseData(CreateReleaseRetainOptions(deployments, environments, projects, Array.Empty<Release>(), numOfReleasesToKeep)) {TestName = "Release"};
    }

    private static IEnumerable<TestCaseData> OrphanedReleasesTestCases()
    {
        var project = CreateProject();
        var release = CreateRelease(project.Id, UtcNow);
        var environment = CreateEnvironment();
        var numOfReleasesToKeep = 2;

        var environments = new List<Environment> {environment};
        var releases = new List<Release> {release};
        var projects = new List<Project> {project};

        var releaseWithoutProject = CreateRelease(null, UtcNow);

        var deploymentForReleaseWithoutProject = CreateDeployment(releaseWithoutProject.Id, environment.Id, UtcNow);
        var deploymentWithDifferentRelease = DeploymentBuilder.CreateRandom().Build();

        yield return new TestCaseData(CreateReleaseRetainOptions(new List<Deployment> {deploymentForReleaseWithoutProject}, environments, projects, new List<Release> {releaseWithoutProject}, numOfReleasesToKeep)) {TestName = "ReleaseWithoutProject"};
        yield return new TestCaseData(CreateReleaseRetainOptions(new List<Deployment> {deploymentWithDifferentRelease}, environments, projects, releases, numOfReleasesToKeep)) {TestName = "ReleaseWithoutDeployment"};
    }

    [Test]
    public void RetainReleases_KeepsReleaseWithSameDeploymentTime_ToTheSameEnvironment()
    {
        // Arrange
        var project = CreateProject();
        var release1 = CreateRelease(project.Id, UtcNow);
        var release2 = CreateRelease(project.Id, UtcNow.AddHours(1));
        var environment = CreateEnvironment();

        var deployment1 = CreateDeployment(release2.Id, environment.Id, UtcNow);
        var deployment2 = CreateDeployment(release1.Id, environment.Id, UtcNow);

        var deployments = new List<Deployment> {deployment1, deployment2};
        var environments = new List<Environment> {environment};
        var releases = new List<Release> {release1, release2};
        var projects = new List<Project> {project};

        var options = CreateReleaseRetainOptions(deployments, environments, projects, releases, 1);

        var expectedResult = new[] {release2};

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
        var project = CreateProject();
        var release1 = CreateRelease(project.Id, UtcNow);
        var release2 = CreateRelease(project.Id, UtcNow.AddHours(1));
        var environment = CreateEnvironment();

        var deployment1 = CreateDeployment(release2.Id, environment.Id, UtcNow);
        var deployment2 = CreateDeployment(release1.Id, environment.Id, UtcNow.AddHours(1));

        var deployments = new List<Deployment> {deployment1, deployment2};
        var environments = new List<Environment> {environment};
        var releases = new List<Release> {release1, release2};
        var projects = new List<Project> {project};

        var options = CreateReleaseRetainOptions(deployments, environments, projects, releases, 1);

        var expectedResult = new[] {release1};

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
        var project = CreateProject();
        var release = CreateRelease(project.Id, UtcNow);
        var environment1 = CreateEnvironment();
        var environment2 = CreateEnvironment();

        var deployment1 = CreateDeployment(release.Id, environment1.Id, UtcNow);
        var deployment2 = CreateDeployment(release.Id, environment1.Id, UtcNow.AddHours(1));
        var deployment3 = CreateDeployment(release.Id, environment2.Id, UtcNow);
        var deployment4 = CreateDeployment(release.Id, environment2.Id, UtcNow.AddHours(1));

        var deployments = new List<Deployment> {deployment1, deployment2, deployment4, deployment3};
        var environments = new List<Environment> {environment1, environment2};
        var releases = new List<Release> {release};
        var projects = new List<Project> {project};

        var options = CreateReleaseRetainOptions(deployments, environments, projects, releases, 1);

        var expectedResult = new[] {release, release};

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
    public void ShouldNotThrowException_WhenCollectionsAreEmpty(ReleaseRetainOptions options)
    {
        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEmpty();
        _logger.Logs.Count.Should().Be(0);
    }

    [Test]
    [TestCaseSource(nameof(OrphanedReleasesTestCases))]
    public void ShouldNotThrowException_WhenReleaseIsOrphaned(ReleaseRetainOptions options)
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
        var project = CreateProject();
        var release = CreateRelease(project.Id, UtcNow);
        var environment = CreateEnvironment();

        var deployment = CreateDeployment(release.Id, null, UtcNow);

        var deployments = new List<Deployment> {deployment};
        var environments = new List<Environment> {environment};
        var releases = new List<Release> {release};
        var projects = new List<Project> {project};

        var options = CreateReleaseRetainOptions(deployments, environments, projects, releases, 1);

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
        var project = CreateProject();
        var release1 = CreateRelease(project.Id, UtcNow);
        var release2 = CreateRelease(project.Id, UtcNow.AddHours(1));
        var environment = CreateEnvironment();

        var deployment1 = CreateDeployment(release1.Id, environment.Id, UtcNow);
        var deployment2 = CreateDeployment(release2.Id, environment.Id, UtcNow);

        var deployments = new List<Deployment> {deployment1, deployment2};
        var environments = new List<Environment> {environment};
        var releases = new List<Release> {release1, release2};
        var projects = new List<Project> {project};

        var options = CreateReleaseRetainOptions(deployments, environments, projects, releases, 3);

        var expectedResult = new[] {release1, release2};

        // Act
        var result = _systemUnderTest.RetainReleases(options);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Logs.Count.Should().Be(2);
        AssertLogMessage(release1.Id, environment.Id);
        AssertLogMessage(release2.Id, environment.Id);
    }
}