using ReleaseRetainer.Test.Helpers;

namespace ReleaseRetainer.Test.Builders;

using Environment = Entities.Environment;

public class EnvironmentTestBuilder : GenericTestBuilder<Environment>
{
    public override EnvironmentTestBuilder CreateRandom()
    {
        Instance.Id = $"Environment-{Guid.NewGuid()}";
        Instance.Name = $"Name-{TestDataGenerator.GetRandomNumber(1, 100)}";

        return this;
    }
}