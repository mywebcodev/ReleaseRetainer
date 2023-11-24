using ReleaseRetainer.Entities;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Models;

public record RetainReleaseOptions
{
    public IEnumerable<Deployment> Deployments { get; set; }
    public IEnumerable<Environment> Environments { get; set; }
    public IEnumerable<Project> Projects { get; set; }
    public IEnumerable<Release> Releases { get; set; }
    public int NumOfReleasesToKeep { get; set; }
}