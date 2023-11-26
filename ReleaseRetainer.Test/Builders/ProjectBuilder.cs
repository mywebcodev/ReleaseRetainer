using ReleaseRetainer.Entities;

namespace ReleaseRetainer.Test.Builders;

public class ProjectBuilder : GenericBuilder<Project>
{
    public override ProjectBuilder CreateRandom()
    {
        Instance.Id = $"Project-{Guid.NewGuid()}";
        Instance.Name = $"Name-{Guid.NewGuid()}";

        return this;
    }
}