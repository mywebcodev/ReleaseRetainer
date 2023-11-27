using ReleaseRetainer.Dtos;

namespace ReleaseRetainer.Test.Builders;

public class ProjectBuilder : GenericBuilder<ProjectDto>
{
    public override ProjectBuilder CreateRandom()
    {
        Instance.Id = $"Project-{Guid.NewGuid()}";
        Instance.Name = $"Name-{Guid.NewGuid()}";

        return this;
    }
}