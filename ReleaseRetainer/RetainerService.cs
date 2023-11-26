using ReleaseRetainer.Criteria;
using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;

namespace ReleaseRetainer;

/// <summary>
/// Service responsible for retaining a specified number of releases based on deployment history.
/// </summary>
public interface IRetainerService
{
    /// <summary>
    /// Retains releases based on the specified options, considering deployment history.
    /// </summary>
    /// <param name="options">Options specifying the Deployments, Environments, Projects, Releases and NumOfReleasesToKeep.</param>
    /// <returns>An IEnumerable of unique retained releases that have most recently been deployed.</returns>
    IEnumerable<Release> RetainReleases(RetainReleaseOptions options);
}

public class RetainerService(IReleaseRetentionStrategy releaseRetentionStrategy) : IRetainerService
{
    public IEnumerable<Release> RetainReleases(RetainReleaseOptions options)
    {
        return releaseRetentionStrategy.RetainReleases(options);
    }
}