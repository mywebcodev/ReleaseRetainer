using ReleaseRetainer.Entities;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Models;

public record ReleaseRetainOptions
{
    private IEnumerable<Deployment>? _deployments;
    private IEnumerable<Environment>? _environments;
    private IEnumerable<Project>? _projects;
    private IEnumerable<Release>? _releases;
    private int _numOfReleasesToKeep;

    public IEnumerable<Deployment> Deployments
    {
        get => _deployments!;
        set => _deployments = value ?? throw new ArgumentNullException(nameof(Deployments), $@"{nameof(Deployments)} list cannot be null.");
    }

    public IEnumerable<Environment> Environments
    {
        get => _environments!;
        set => _environments = value ?? throw new ArgumentNullException(nameof(Environments), $@"{nameof(Environments)} list cannot be null.");
    }

    public IEnumerable<Project> Projects
    {
        get => _projects!;
        set => _projects = value ?? throw new ArgumentNullException(nameof(Projects), $@"{nameof(Projects)} list cannot be null.");
    }

    public IEnumerable<Release> Releases
    {
        get => _releases!;
        set => _releases = value ?? throw new ArgumentNullException(nameof(Releases), $@"{nameof(Releases)} list cannot be null.");
    }

    public int NumOfReleasesToKeep
    {
        get => _numOfReleasesToKeep;
        set => _numOfReleasesToKeep = value > 0 ? value : throw new ArgumentException($@"{nameof(NumOfReleasesToKeep)} must be greater than zero.", nameof(NumOfReleasesToKeep));
    }
}