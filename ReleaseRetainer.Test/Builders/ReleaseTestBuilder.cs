using ReleaseRetainer.Entities;
using ReleaseRetainer.Test.Helpers;

namespace ReleaseRetainer.Test.Builders;

public class ReleaseTestBuilder : GenericTestBuilder<Release>
{
    public override ReleaseTestBuilder CreateRandom()
    {
        Instance.Id = $"Release-{Guid.NewGuid()}";
        Instance.ProjectId = $"ProjectId-{Guid.NewGuid()}";
        Instance.Version = $"{TestDataGenerator.GetRandomNumber(1, 100)}.{TestDataGenerator.GetRandomNumber(1, 100)}.{TestDataGenerator.GetRandomNumber(1, 100)}";
        Instance.Created = DateTime.UtcNow.AddHours(TestDataGenerator.GetRandomNumber(-24, 24));

        return this;
    }
}