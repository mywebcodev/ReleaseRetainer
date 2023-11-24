using ReleaseRetainer.Entities;

namespace ReleaseRetainer.Test.Builders;

public record ReleaseTestBuilder
{
    private Release _release = new ();

    public ReleaseTestBuilder WithId(string id)
    {
        _release.Id = id;
        return this;
    }

    public ReleaseTestBuilder WithProjectId(string projectId)
    {
        _release.ProjectId = projectId;
        return this;
    }

    public ReleaseTestBuilder WithVersion(string version)
    {
        _release.Version = version;
        return this;
    }

    public ReleaseTestBuilder WithCreated(DateTime created)
    {
        _release.Created = created;
        return this;
    }

    public Release Build()
    {
        var builtRelease = _release;
        _release = new Release();
        return builtRelease;
    }
}