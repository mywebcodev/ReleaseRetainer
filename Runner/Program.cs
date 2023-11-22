// See https://aka.ms/new-console-template for more information

using ReleaseRetainer;
using ReleaseRetainer.Models;

var projectTask = DataLoader.LoadProjectsAsync();
var releasesTask = DataLoader.LoadReleasesAsync();
var environmentsTask = DataLoader.LoadEnvironmentsAsync();
var deploymentsTask = DataLoader.LoadDeploymentsAsync();

await Task.WhenAll(projectTask, releasesTask, environmentsTask, deploymentsTask);

new Retainer().Retain(new RetainReleaseOptions
{
    Deployments = await deploymentsTask,
    Environments = await environmentsTask,
    Projects = await projectTask,
    Releases = await releasesTask,
    NumOfReleasesToKeep = 1
});
Console.WriteLine("Hello, World!");