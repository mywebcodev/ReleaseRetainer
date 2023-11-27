using System.Text.Json;
using ReleaseRetainer.Dtos;
using ReleaseRetainer.Test.Properties;

namespace ReleaseRetainer;

public class TestDataLoader
{
    private static Task<T?> DeserializeResourceAsync<T>(byte[] resource)
    {
        return Task.Run(() => JsonSerializer.Deserialize<T>(resource));
    }

    public static Task<IEnumerable<ProjectDto>> LoadProjectsAsync()
    {
        return DeserializeResourceAsync<IEnumerable<ProjectDto>>(Resources.Projects);
    }

    public static Task<IEnumerable<ReleaseDto>> LoadReleasesAsync()
    {
        return DeserializeResourceAsync<IEnumerable<ReleaseDto>>(Resources.Releases);
    }

    public static Task<IEnumerable<EnvironmentDto>> LoadEnvironmentsAsync()
    {
        return DeserializeResourceAsync<IEnumerable<EnvironmentDto>>(Resources.Environments);
    }

    public static Task<IEnumerable<DeploymentDto>> LoadDeploymentsAsync()
    {
        return DeserializeResourceAsync<IEnumerable<DeploymentDto>>(Resources.Deployments);
    }
}