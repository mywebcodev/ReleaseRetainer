﻿using ReleaseRetainer.Entities;
using ReleaseRetainer.Properties;
using System.Text.Json;
using Environment = ReleaseRetainer.Entities.Environment;

namespace ReleaseRetainer;

public class DataLoader
{
    private static Task<T?> DeserializeResourceAsync<T>(byte[] resource)
    {
        return Task.Run(() => JsonSerializer.Deserialize<T>(resource));
    }

    public static Task<IEnumerable<Project>> LoadProjectsAsync()
    {
        return DeserializeResourceAsync<IEnumerable<Project>>(Resources.Projects);
    }

    public static Task<IEnumerable<Release>> LoadReleasesAsync()
    {
        return DeserializeResourceAsync<IEnumerable<Release>>(Resources.Releases);
    }

    public static Task<IEnumerable<Environment>> LoadEnvironmentsAsync()
    {
        return DeserializeResourceAsync<IEnumerable<Environment>>(Resources.Environments);
    }

    public static Task<IEnumerable<Deployment>> LoadDeploymentsAsync()
    {
        return DeserializeResourceAsync<IEnumerable<Deployment>>(Resources.Deployments);
    }
}