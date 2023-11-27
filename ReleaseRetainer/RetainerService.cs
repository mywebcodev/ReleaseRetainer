using ReleaseRetainer.Dtos;
using ReleaseRetainer.Models;
using ReleaseRetainer.Strategies;

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
    IEnumerable<ReleaseDto> RetainReleases(ReleaseRetainOptions options);
}

public class RetainerService(IReleaseRetentionStrategy releaseRetentionStrategy) : IRetainerService
{
    public IEnumerable<ReleaseDto> RetainReleases(ReleaseRetainOptions options)
    {
        return releaseRetentionStrategy.RetainReleases(options);
    }
}