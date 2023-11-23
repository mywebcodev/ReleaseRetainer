using AutoFixture;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Test;

[TestFixture]
public class RetainerTests
{
    private RetainerService _systemUnderTest;
    private MockLogger<RetainerService> _logger;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
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
        var options = _fixture.Build<RetainReleaseOptions>()
                              .With(p => p.NumOfReleasesToKeep, numOfReleasesToKeep)
                              .Create();

        // Act
        // Assert
        var exception = Assert.Throws<ArgumentException>(() => _systemUnderTest.RetainReleases(options));

        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Test]
    public void RetainReleases_KeepsReleaseWithSameDeploymentTime_ToTheSameEnvironment()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var deployments = new List<Deployment>
        {
            new()
            {
                Id = "Deployment-2",
                ReleaseId = "Release-1",
                EnvironmentId = "Environment-1",
                DeployedAt = now
            },
            new()
            {
                Id = "Deployment-1",
                ReleaseId = "Release-2",
                EnvironmentId = "Environment-1",
                DeployedAt = now
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

        var releases = new List<Release>
        {
            new()
            {
                Id = "Release-1",
                ProjectId = "Project-1",
                Version = "1.0.0",
                Created = now
            },
            new()
            {
                Id = "Release-2",
                ProjectId = "Project-1",
                Version = "1.0.1",
                Created = now.AddHours(1),
            }
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
        _systemUnderTest.RetainReleases(options);

        // Assert
        _logger.Logs.Count.Should().Be(1);
        _logger.Logs.Should().Contain("'Release-2' kept because it was most recently deployed to 'Environment-1'");
    }

    [Test]
    public void RetainReleases_KeepsReleaseWithDifferentDeploymentTime_ToTheSameEnvironment()
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


        var releases = new List<Release>
        {
            new()
            {
                Id = "Release-2",
                ProjectId = "Project-1",
                Version = "1.0.1",
                Created = DateTime.Parse("2000-01-01T09:00:00"),
            },
            new()
            {
                Id = "Release-1",
                ProjectId = "Project-1",
                Version = "1.0.0",
                Created = DateTime.Parse("2000-01-01T08:00:00")
            }
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
        _systemUnderTest.RetainReleases(options);

        // Assert
        _logger.LogInformation("'{ReleaseId}' kept because it was most recently deployed to '{EnvironmentId}'", "Release-1", "Environment-1");
    }

    [Test]
    public void RetainReleases_KeepsReleasesWithSameId_ForDifferentProjectAndEnvironmentCombinations()
    {
        // Arrange
        var deployments = new List<Deployment>
        {
            new()
            {
                Id = "Deployment-1",
                ReleaseId = "Release-1",
                EnvironmentId = "Environment-1",
                DeployedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "Deployment-2",
                ReleaseId = "Release-1",
                EnvironmentId = "Environment-1",
                DeployedAt = DateTime.UtcNow.AddHours(2)
            },
            new()
            {
                Id = "Deployment-3",
                ReleaseId = "Release-1",
                EnvironmentId = "Environment-2",
                DeployedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "Deployment-4",
                ReleaseId = "Release-1",
                EnvironmentId = "Environment-2",
                DeployedAt = DateTime.UtcNow.AddHours(3)
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
                Created = DateTime.UtcNow
            }
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
        _systemUnderTest.RetainReleases(options);

        // Assert
        _logger.Logs.Count.Should().Be(2);
        _logger.Logs.Should().Contain("'Release-1' kept because it was most recently deployed to 'Environment-1'");
        _logger.Logs.Should().Contain("'Release-1' kept because it was most recently deployed to 'Environment-2'");
    }
}