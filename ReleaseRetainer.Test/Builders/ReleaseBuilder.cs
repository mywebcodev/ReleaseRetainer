using ReleaseRetainer.Dtos;
using ReleaseRetainer.Test.Helpers;

namespace ReleaseRetainer.Test.Builders;

public class ReleaseBuilder : GenericBuilder<ReleaseDto>
{
    public override ReleaseBuilder CreateRandom()
    {
        Instance.Id = $"Release-{Guid.NewGuid()}";
        Instance.ProjectId = $"ProjectId-{Guid.NewGuid()}";
        Instance.Version = $"{TestDataGenerator.GetRandomNumber(1, 100)}.{TestDataGenerator.GetRandomNumber(1, 100)}.{TestDataGenerator.GetRandomNumber(1, 100)}";
        Instance.Created = DateTime.UtcNow.AddHours(TestDataGenerator.GetRandomNumber(-24, 24));

        return this;
    }
}