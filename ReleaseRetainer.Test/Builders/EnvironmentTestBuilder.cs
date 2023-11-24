namespace ReleaseRetainer.Test.Builders;
using Environment = Entities.Environment;

public record EnvironmentTestBuilder
{
    private Environment _environment = new ();

    public EnvironmentTestBuilder WithId(string id)
    {
        _environment.Id = id;
        return this;
    }

    public EnvironmentTestBuilder WithName(string name)
    {
        _environment.Name = name;
        return this;
    }

    public Environment Build()
    {
        var builtEnvironment = _environment;
        _environment = new Environment();
        return builtEnvironment;
    }
}