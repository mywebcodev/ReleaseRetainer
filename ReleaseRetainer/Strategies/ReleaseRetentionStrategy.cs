using Microsoft.Extensions.Logging;
using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;

namespace ReleaseRetainer.Strategies;

public interface IReleaseRetentionStrategy
{
    IEnumerable<Release> RetainReleases(ReleaseRetainOptions options);
}

// https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-library-authors
// Using Microsoft.Extensions.Logging.ILogger is preferable for logging in a public .NET library
// because it allows the consumers of your library to plug in their own logging implementation.
// It provides a flexible and extensible way to handle logging.
public class ReleaseRetentionStrategy(ILogger<ReleaseRetentionStrategy> logger) : IReleaseRetentionStrategy
{
    // Create a dictionary for deployments per environment, ordered by DeployedAt
    private static Dictionary<(string ReleaseId, string EnvironmentId), List<Deployment>> CreateReleaseDeploymentsPerEnvironmentMap(IEnumerable<Deployment> deployments)
    {
        return deployments
               .GroupBy(d => (d.ReleaseId, d.EnvironmentId))
               .ToDictionary(k => k.Key, v => v.OrderByDescending(d => d.DeployedAt).ToList());
    }

    private void LogRetainedReleases(IEnumerable<Release> releases, string environmentId)
    {
        foreach (var release in releases)
        {
            // Log if the release has been retained
            // Log if the release was already retained but deployed on a different environment
            logger.LogInformation("'{ReleaseId}' kept because it was most recently deployed to '{EnvironmentId}'", release.Id, environmentId);
        }
    }

    // For each **project**/**environment** combination,
    // retain `n` **releases** that have been most recently deployed,
    // where `n` is the number of releases to keep.

    // Note: We may end up with duplicate releases following the rule above.
    // For instance, a release with the same Id can be deployed within different projects or environments.
    // To avoid duplicates, an additional check is needed:
    // If a release with the same Id is already retained, skip retaining it.
    public IEnumerable<Release> RetainReleases(ReleaseRetainOptions options)
    {
        var releases = options.Releases;
        var deployments = options.Deployments;
        var projects = options.Projects;
        var environments = options.Environments;
        var numOfReleasesToKeep = options.NumOfReleasesToKeep;
        var releasesByProjectIdLookup = releases.ToLookup(r => r.ProjectId);

        var releaseDeploymentsPerEnvironment = CreateReleaseDeploymentsPerEnvironmentMap(deployments);

        var retainedReleases = new List<Release>();

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
                                              .Take(numOfReleasesToKeep)
                                              .ToList();

                LogRetainedReleases(retainedProjectReleases, environment.Id);
                retainedReleases.AddRange(retainedProjectReleases);
            }
        }

        return retainedReleases;
    }
}