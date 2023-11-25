using Microsoft.Extensions.Logging;
using ReleaseRetainer.Entities;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer.Criteria;

public interface IReleaseRetentionStrategy
{
    IEnumerable<Release> RetainReleases(IEnumerable<Release> releases, IEnumerable<Deployment> deployments, Project project, Environment environment, int numOfReleasesToKeep);
}

public class ReleaseRetentionStrategy(ILogger<ReleaseRetentionStrategy> logger) : IReleaseRetentionStrategy
{
    // For each **project**/**environment** combination,
    // retain `n` **releases** that have been most recently deployed,
    // where `n` is the number of releases to keep.

    // Note: We may end up with duplicate releases following the rule above.
    // For instance, a release with the same Id can be deployed within different projects or environments.
    // To avoid duplicates, an additional check is needed:
    // If a release with the same Id is already retained, skip retaining it.
    public IEnumerable<Release> RetainReleases(IEnumerable<Release> releases, IEnumerable<Deployment> deployments, Project project, Environment environment, int numOfReleasesToKeep)
    {
        var releasesByProjectIdLookup = releases.ToLookup(r => r.ProjectId);

        // Create a dictionary for deployments per environment, ordered by DeployedAt
        var releaseDeploymentsPerEnvironment = deployments
                                               .GroupBy(d => (d.ReleaseId, d.EnvironmentId))
                                               .ToDictionary(k => k.Key, v => v.OrderByDescending(d => d.DeployedAt).ToList());

        // Get releases for the current project and environment combination
        var retainedProjectReleases = releasesByProjectIdLookup[project.Id]
                                      // Filter releases that have deployments in the current environment
                                      .Where(r => releaseDeploymentsPerEnvironment.ContainsKey((r.Id, project.Id)))
                                      // Order releases by the most recent deployment in the current environment,
                                      .OrderByDescending(r => releaseDeploymentsPerEnvironment[(r.Id, environment.Id)].First().DeployedAt)
                                      // then by the release creation date in descending order
                                      .ThenByDescending(r => r.Created)
                                      // Use DistinctBy to avoid duplicated releases based on their Id
                                      .DistinctBy(r => r.Id)
                                      // Take the specified number of releases to retain
                                      .Take(numOfReleasesToKeep)
                                      .ToList();

        foreach (var release in retainedProjectReleases)
        {
            // Log if the release has been retained
            // Log if the release was already retained but deployed on a different environment
            logger.Log(LogLevel.Information, "'{ReleaseId}' kept because it was most recently deployed to '{EnvironmentId}'", release.Id, environment.Id);
        }

        return retainedProjectReleases;
    }
}