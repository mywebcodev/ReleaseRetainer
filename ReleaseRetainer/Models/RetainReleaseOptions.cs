using ReleaseRetainer.Entities;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Models;

public record RetainReleaseOptions
{
    public required IEnumerable<Deployment> Deployments { get; init; }
    public required IEnumerable<Environment> Environments { get; init; }
    public required IEnumerable<Project> Projects { get; init; }
    public required IEnumerable<Release> Releases { get; init; }
    public required int NumOfReleasesToKeep { get; init; }
}