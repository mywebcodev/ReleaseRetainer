using ReleaseRetainer.Entities;
using ReleaseRetainer.Test.Helpers;

namespace ReleaseRetainer.Test.Builders;

public class DeploymentTestBuilder : GenericTestBuilder<Deployment>
{
    public override DeploymentTestBuilder CreateRandom()
    {
        Instance.Id = $"Deployment-{Guid.NewGuid()}";
        Instance.ReleaseId = $"Release-{Guid.NewGuid()}";
        Instance.EnvironmentId = $"Environment-{Guid.NewGuid()}";
        Instance.DeployedAt = DateTime.UtcNow.AddHours(TestDataGenerator.GetRandomNumber(-24, 24));

        return this;
    }
}