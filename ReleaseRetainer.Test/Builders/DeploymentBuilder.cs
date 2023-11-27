using ReleaseRetainer.Dtos;
using ReleaseRetainer.Test.Helpers;

namespace ReleaseRetainer.Test.Builders;

public class DeploymentBuilder : GenericBuilder<DeploymentDto>
{
    public override DeploymentBuilder CreateRandom()
    {
        Instance.Id = $"Deployment-{Guid.NewGuid()}";
        Instance.ReleaseId = $"Release-{Guid.NewGuid()}";
        Instance.EnvironmentId = $"Environment-{Guid.NewGuid()}";
        Instance.DeployedAt = DateTime.UtcNow.AddHours(TestDataGenerator.GetRandomNumber(-24, 24));

        return this;
    }
}