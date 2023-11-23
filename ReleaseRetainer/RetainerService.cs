using Microsoft.Extensions.Logging;
using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;

namespace ReleaseRetainer;

public interface IRetainerService
{
    IEnumerable<Release> RetainReleases(RetainReleaseOptions options);
}

public class RetainerService(ILogger<RetainerService>? logger = null) : IRetainerService
{
    public IEnumerable<Release> RetainReleases(RetainReleaseOptions options)
    {
        var numOfReleasesToKeep = options.NumOfReleasesToKeep;
        var deployments = options.Deployments;
        var environments = options.Environments;
        var projects = options.Projects;
        var result = new List<Release>();

        var releasesByProjectIdLookup = options.Releases.ToLookup(r => r.ProjectId);
        var releaseDeploymentsPerEnvironment = deployments
                                               .GroupBy(d => (d.ReleaseId, d.EnvironmentId))
                                               .ToDictionary(k => k.Key, v => v.OrderByDescending(d => d.DeployedAt).ToList());

        foreach (var project in projects)
        {
            foreach (var environment in environments)
            {
                var retainedProjectReleases = releasesByProjectIdLookup[project.Id]
                                              .Where(r => releaseDeploymentsPerEnvironment.ContainsKey((r.Id, environment.Id)))
                                              .DistinctBy(r => r.Id)
                                              .OrderByDescending(r => releaseDeploymentsPerEnvironment[(r.Id, environment.Id)].First().DeployedAt)
                                              .Take(numOfReleasesToKeep);

                foreach (var release in retainedProjectReleases)
                {
                    result.Add(release);

                    var deployment = releaseDeploymentsPerEnvironment[(release.Id, environment.Id)].First();
                    logger?.LogInformation("'{ReleaseId}' kept because it was most recently deployed to '{EnvironmentId}'", release.Id, deployment.EnvironmentId);
                }
            }
        }

        return result;
    }
}