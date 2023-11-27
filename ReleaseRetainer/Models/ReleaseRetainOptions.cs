using ReleaseRetainer.Dtos;

namespace ReleaseRetainer.Models;

public record ReleaseRetainOptions
{
    private IEnumerable<DeploymentDto>? _deployments;
    private IEnumerable<EnvironmentDto>? _environments;
    private IEnumerable<ProjectDto>? _projects;
    private IEnumerable<ReleaseDto>? _releases;
    private int _numOfReleasesToKeep;

    public IEnumerable<DeploymentDto> Deployments
    {
        get => _deployments!;
        set => _deployments = value ?? throw new ArgumentNullException(nameof(Deployments), $@"{nameof(Deployments)} list cannot be null.");
    }

    public IEnumerable<EnvironmentDto> Environments
    {
        get => _environments!;
        set => _environments = value ?? throw new ArgumentNullException(nameof(Environments), $@"{nameof(Environments)} list cannot be null.");
    }

    public IEnumerable<ProjectDto> Projects
    {
        get => _projects!;
        set => _projects = value ?? throw new ArgumentNullException(nameof(Projects), $@"{nameof(Projects)} list cannot be null.");
    }

    public IEnumerable<ReleaseDto> Releases
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