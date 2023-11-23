using Microsoft.Extensions.Logging;
using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;

namespace ReleaseRetainer;

public interface IRetainerService
{
    IEnumerable<Release> RetainReleases(RetainReleaseOptions options);
}

public class RetainerService : IRetainerService
{
    private readonly ILogger<RetainerService> _logger;

    public RetainerService(ILogger<RetainerService> logger = null)
    {
        _logger = logger;
    }

    public IEnumerable<Release> RetainReleases(RetainReleaseOptions options)
    {
        var projectByIdMap = options.Projects.ToDictionary(k => k.Id);
        var releasesByProjectIdLookup = options.Releases.ToLookup(r => r.ProjectId); // releases can be orphaned, use lookup for handling nullable ProjectId gracefully
        var environmentsByIdMap = options.Environments.GroupBy(d => d.Id).ToDictionary(k => k.Key);
        var deploymentsByReleaseIdMap = options.Deployments.GroupBy(d => d.ReleaseId).ToDictionary(k => k.Key, v => v.OrderByDescending(d => d.DeployedAt).ToArray());
        var numOfReleasesToKeep = options.NumOfReleasesToKeep;
        var retainedReleases = new List<Release>(numOfReleasesToKeep);

        foreach (var releasesGroupingByProjectId in releasesByProjectIdLookup)
        {
            var projectId = releasesGroupingByProjectId.Key;
            var projectReleases = releasesGroupingByProjectId.ToList();
            
            for (var i = 0; i < projectReleases.Count; i++)
            {

                var release = projectReleases[i];
                var project = projectId != null && projectByIdMap.TryGetValue(projectId, out var p) ? p : null;
                var recentDeployment = deploymentsByReleaseIdMap.TryGetValue(release.Id, out var d) ? d[0] : null;
         
                if (i < numOfReleasesToKeep && recentDeployment != null)
                {
                    retainedReleases.Add(release);
                    _logger.LogInformation("'{ReleaseId}' kept because it was most recently deployed to '{EnvironmentId}'", release.Id, recentDeployment.EnvironmentId);
                }
                // retain release if release is not orphaned
                else if (i < numOfReleasesToKeep && project != null)
                {
                    // retain release
                    retainedReleases.Add(release);
                    _logger.LogInformation("'{ReleaseId}' kept because it wasn't deployed", release.Id);
                }
            }
        }

        return retainedReleases;
    }
}