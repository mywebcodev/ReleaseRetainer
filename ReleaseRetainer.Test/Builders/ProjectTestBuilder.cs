using ReleaseRetainer.Entities;

namespace ReleaseRetainer.Test.Builders;

public record ProjectTestBuilder
{
    private Project _project = new();

    public ProjectTestBuilder WithId(string id)
    {
        _project.Id = id;
        return this;
    }

    public ProjectTestBuilder WithName(string name)
    {
        _project.Name = name;
        return this;
    }

    public Project Build()
    {
        var builtProject = _project;
        _project = new Project();
        return builtProject;
    }
}