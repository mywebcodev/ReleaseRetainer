using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Test.Builders;

public record RetainReleaseOptionsTestBuilder
{

    // public IEnumerable<Deployment> Deployments { get; init; }
    // public IEnumerable<Environment> Environments { get; init; }
    // public IEnumerable<Project> Projects { get; init; }
    // public IEnumerable<Release> Releases { get; init; }
    // public int NumOfReleasesToKeep { get; init; }
    private RetainReleaseOptions _options = new();

    public RetainReleaseOptionsTestBuilder WithDeployments(IEnumerable<Deployment> deployments)
    {
        _options.Deployments = deployments;
        return this;
    }

    public RetainReleaseOptionsTestBuilder WithEnvironments(IEnumerable<Environment> environments)
    {
        _options.Environments = environments;
        return this;
    }

    public RetainReleaseOptionsTestBuilder WithProjects(IEnumerable<Project> projects)
    {
        _options.Projects = projects;
        return this;
    }

    public RetainReleaseOptionsTestBuilder WithReleases(IEnumerable<Release> releases)
    {
        _options.Releases = releases;
        return this;
    }

    public RetainReleaseOptionsTestBuilder WithNumOfReleasesToKeep(int numOfReleasesToKeep)
    {
        _options.NumOfReleasesToKeep = numOfReleasesToKeep;
        return this;
    }

    public RetainReleaseOptions Build()
    {
        var builtOptions = _options;
        _options = new RetainReleaseOptions();
        return builtOptions;
    }
}