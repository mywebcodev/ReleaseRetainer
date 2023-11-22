using FluentAssertions;
using NUnit.Framework;
using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Test;

[TestFixture]
public class RetainerTests
{
    private Retainer _systemUnderTest;

    [OneTimeSetUp]
    public void SetUp()
    {
        _systemUnderTest = new Retainer();
    }

    [Test]
    public void ReleasesDeployedToTheSameEnvironment()
    {
        // Arrange
        var deployments = new List<Deployment>
        {
            new()
            {
                Id = "Deployment-2",
                ReleaseId = "Release-1",
                EnvironmentId = "Environment-1",
                DeployedAt = DateTime.Parse("2000-01-01T11:00:00")
            },
            new()
            {
                Id = "Deployment-1",
                ReleaseId = "Release-1",
                EnvironmentId = "Environment-1",
                DeployedAt = DateTime.Parse("2000-01-01T10:00:00")
            }
        };

        var environments = new List<Environment>
        {
            new()
            {
                Id = "Environment-1",
                Name = "Staging"
            }
        };

        var expectedRelease = new Release()
        {
            Id = "Release-1",
            ProjectId = "Project-1",
            Version = "1.0.0",
            Created = DateTime.Parse("2000-01-01T08:00:00"),
        };

        var releases = new List<Release>
        {
            new()
            {
                Id = "Release-2",
                ProjectId = "Project-1",
                Version = "1.0.1",
                Created = DateTime.Parse("2000-01-01T09:00:00"),
            },
            expectedRelease
        };

        var projects = new List<Project>
        {
            new()
            {
                Id = "Project-1",
                Name = "Random Quotes"
            }
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
        var result = _systemUnderTest.Retain(options);

        // Assert
        result.Should().Contain(expectedRelease);
    }

        [Test]
    public void ReleasesDeployedToTheDifferentEnvironment()
    {
        // Arrange
        var deployments = new List<Deployment>
        {
            new()
            {
                Id = "Deployment-2",
                ReleaseId = "Release-1",
                EnvironmentId = "Environment-1",
                DeployedAt = DateTime.Parse("2000-01-01T11:00:00")
            },
            new()
            {
                Id = "Deployment-1",
                ReleaseId = "Release-2",
                EnvironmentId = "Environment-2",
                DeployedAt = DateTime.Parse("2000-01-01T10:00:00")
            }
        };

        var environments = new List<Environment>
        {
            new()
            {
                Id = "Environment-1",
                Name = "Staging"
            },
            new()
            {
                Id = "Environment-2",
                Name = "Production"
            }
        };

        var releases = new List<Release>
        {
            new()
            {
                Id = "Release-1",
                ProjectId = "Project-1",
                Version = "1.0.0",
                Created = DateTime.Parse("2000-01-01T08:00:00"),
            }, 
            new()
            {
                Id = "Release-2",
                ProjectId = "Project-1",
                Version = "1.0.1",
                Created = DateTime.Parse("2000-01-01T09:00:00"),
            },
        };

        var projects = new List<Project>
        {
            new()
            {
                Id = "Project-1",
                Name = "Random Quotes"
            }
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
        var result = _systemUnderTest.Retain(options);

        // Assert
        result.Should().Contain(releases);
    }
}