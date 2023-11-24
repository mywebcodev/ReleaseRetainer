using ReleaseRetainer.Entities;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Models;

public record RetainReleaseOptions
{
    private readonly IEnumerable<Deployment>? _deployments;
    private readonly IEnumerable<Environment>? _environments;
    private readonly IEnumerable<Project>? _projects;
    private readonly IEnumerable<Release>? _releases;
    private readonly int _numOfReleasesToKeep;

    public IEnumerable<Deployment> Deployments
    {
        get => _deployments!;
        init => _deployments = value ?? throw new ArgumentNullException(nameof(Deployments), $@"{nameof(Deployments)} list cannot be null.");
    }

    public IEnumerable<Environment> Environments
    {
        get => _environments!;
        init => _environments = value ?? throw new ArgumentNullException(nameof(Environments), $@"{nameof(Environments)} list cannot be null.");
    }

    public IEnumerable<Project> Projects
    {
        get => _projects!;
        init => _projects = value ?? throw new ArgumentNullException(nameof(Projects), $@"{nameof(Projects)} list cannot be null.");
    }

    public IEnumerable<Release> Releases
    {
        get => _releases!;
        init => _releases = value ?? throw new ArgumentNullException(nameof(Releases), $@"{nameof(Releases)} list cannot be null.");
    }

    public int NumOfReleasesToKeep
    {
        get => _numOfReleasesToKeep;
        init => _numOfReleasesToKeep = value > 0 ? value : throw new ArgumentException($@"{nameof(NumOfReleasesToKeep)} must be greater than zero.", nameof(NumOfReleasesToKeep));
    }
}