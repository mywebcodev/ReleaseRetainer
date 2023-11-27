using ReleaseRetainer.Dtos;
using ReleaseRetainer.Test.Helpers;

namespace ReleaseRetainer.Test.Builders;

public class EnvironmentBuilder : GenericBuilder<EnvironmentDto>
{
    public override EnvironmentBuilder CreateRandom()
    {
        Instance.Id = $"Environment-{Guid.NewGuid()}";
        Instance.Name = $"Name-{TestDataGenerator.GetRandomNumber(1, 100)}";

        return this;
    }
}