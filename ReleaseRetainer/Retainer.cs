using ReleaseRetainer.Entities;
using ReleaseRetainer.Models;

namespace ReleaseRetainer;

public class Retainer
{
    public IEnumerable<Release> Retain(RetainReleaseOptions options)
    {
        var projectByIdMap = options.Projects.ToDictionary(k => k.Id);
        var releasesByProjectIdLookup = options.Releases.ToLookup(r => r.ProjectId); // releases can be orphaned, use lookup for handling nullable ProjectId gracefully
        var environmentsByIdMap = options.Environments.GroupBy(d => d.Id).ToDictionary(k => k.Key);
        var deploymentsByReleaseIdMap = options.Deployments.GroupBy(d => d.ReleaseId).ToDictionary(k => k.Key, v => v.OrderByDescending(d => d.DeployedAt).ToList());
        var retainedReleases = new List<Release>(options.NumOfReleasesToKeep);
        
        foreach (var releasesGroupingByProjectId in releasesByProjectIdLookup)
        {
            var projectId = releasesGroupingByProjectId.Key;
            var projectReleases = releasesGroupingByProjectId.ToList();

            for (var i = 0; i < projectReleases.Count; i++)
            {
                var release = projectReleases[i];

                // retain release if release is not orphaned
                if (i < options.NumOfReleasesToKeep && projectId != null && projectByIdMap.TryGetValue(projectId, out var project))
                {
                    // retain release
                    retainedReleases.Add(release);
                }
                else if (deploymentsByReleaseIdMap.TryGetValue(release.Id, out var releaseDeployments))
                {
                    // get recent deployment
                    var deployment = releaseDeployments[0];

                    environmentsByIdMap.TryGetValue(deployment.EnvironmentId, out var environment);
                    retainedReleases.Add(release);
                }
            }
        }

        return retainedReleases;
    }
}