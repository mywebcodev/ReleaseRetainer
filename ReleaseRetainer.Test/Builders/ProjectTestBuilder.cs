using ReleaseRetainer.Entities;

namespace ReleaseRetainer.Test.Builders;

public class ProjectTestBuilder : GenericTestBuilder<Project>
{
    public override ProjectTestBuilder CreateRandom()
    {
        Instance.Id = $"Project-{Guid.NewGuid()}";
        Instance.Name = $"Name-{Guid.NewGuid()}";

        return this;
    }
}