using Microsoft.Extensions.Logging;
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

// https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-library-authors
// Using Microsoft.Extensions.Logging.ILogger is preferable for logging in a public .NET library
// because it allows the consumers of your library to plug in their own logging implementation.
// It provides a flexible and extensible way to handle logging.
public class RetainerService(IReleaseRetentionStrategy releaseRetentionStrategy) : IRetainerService
{
    public IEnumerable<Release> RetainReleases(RetainReleaseOptions options)
    {
        var numOfReleasesToKeep = options.NumOfReleasesToKeep;
        var deployments = options.Deployments;
        var releases = options.Releases;
        var environments = options.Environments;
        var projects = options.Projects;
        var retainedReleases = new List<Release>();

        foreach (var project in projects)
        {
            foreach (var environment in environments)
            {
                // Get releases for the current project and environment combination
                var projectRetainedReleases = releaseRetentionStrategy.RetainReleases(
                    releases,
                    deployments,
                    project,
                    environment,
                    numOfReleasesToKeep
                );
                retainedReleases.AddRange(projectRetainedReleases);
            }
        }

        return retainedReleases;
    }
}