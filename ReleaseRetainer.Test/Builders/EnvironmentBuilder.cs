using ReleaseRetainer.Test.Helpers;

namespace ReleaseRetainer.Test.Builders;

using Environment = Entities.Environment;

public class EnvironmentBuilder : GenericBuilder<Environment>
{
    public override EnvironmentBuilder CreateRandom()
    {
        Instance.Id = $"Environment-{Guid.NewGuid()}";
        Instance.Name = $"Name-{TestDataGenerator.GetRandomNumber(1, 100)}";

        return this;
    }
}