using Microsoft.Extensions.Logging;
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

// https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-library-authors
// Using Microsoft.Extensions.Logging.ILogger is preferable for logging in a public .NET library
// because it allows the consumers of your library to plug in their own logging implementation.
// It provides a flexible and extensible way to handle logging.
public class RetainerService(ILogger<RetainerService> logger) : IRetainerService
{
    public IEnumerable<Release> RetainReleases(RetainReleaseOptions options)
    {
        var numOfReleasesToKeep = options.NumOfReleasesToKeep;
        var deployments = options.Deployments;
        var environments = options.Environments;
        var projects = options.Projects;
        var retainedReleases = new List<Release>();

        // Create lookup for releases by ProjectId
        var releasesByProjectIdLookup = options.Releases.ToLookup(r => r.ProjectId);

        // Create a dictionary for deployments per environment, ordered by DeployedAt
        var releaseDeploymentsPerEnvironment = deployments
                                               .GroupBy(d => (d.ReleaseId, d.EnvironmentId))
                                               .ToDictionary(k => k.Key, v => v.OrderByDescending(d => d.DeployedAt).ToList());

        // For each **project**/**environment** combination,
        // retain `n` **releases** that have been most recently deployed,
        // where `n` is the number of releases to keep.

        // Note: We may end up with duplicate releases following the rule above.
        // For instance, a release with the same Id can be deployed within different projects or environments.
        // To avoid duplicates, an additional check is needed:
        // If a release with the same Id is already retained, skip retaining it.
        foreach (var project in projects)
        {
            foreach (var environment in environments)
            {
                // Get releases for the current project and environment combination
                var retainedProjectReleases = releasesByProjectIdLookup[project.Id]
                                              // Filter releases that have deployments in the current environment
                                              .Where(r => releaseDeploymentsPerEnvironment.ContainsKey((r.Id, environment.Id)))
                                              // Order releases by the most recent deployment in the current environment,
                                              .OrderByDescending(r => releaseDeploymentsPerEnvironment[(r.Id, environment.Id)].First().DeployedAt)
                                              // then by the release creation date in descending order
                                              .ThenByDescending(r => r.Created)
                                              // Use DistinctBy to avoid duplicated releases based on their Id
                                              .DistinctBy(r => r.Id)
                                              // Take the specified number of releases to retain
                                              .Take(numOfReleasesToKeep);

                foreach (var release in retainedProjectReleases)
                {
                    retainedReleases.Add(release);

                    // Log if the release has been retained
                    // Log if the release was already retained but deployed on a different environment
                    logger.Log(LogLevel.Information, "'{ReleaseId}' kept because it was most recently deployed to '{EnvironmentId}'", release.Id, environment.Id);
                }
            }
        }

        return retainedReleases;
    }
}