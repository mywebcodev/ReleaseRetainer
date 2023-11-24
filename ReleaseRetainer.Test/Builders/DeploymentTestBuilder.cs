using ReleaseRetainer.Entities;

namespace ReleaseRetainer.Test.Builders;

public record DeploymentTestBuilder
{
    private Deployment _deployment = new();

    public DeploymentTestBuilder WithId(string id)
    {
        _deployment.Id = id;
        return this;
    }

    public DeploymentTestBuilder WithReleaseId(string releaseId)
    {
        _deployment.ReleaseId = releaseId;
        return this;
    }

    public DeploymentTestBuilder WithEnvironmentId(string environmentId)
    {
        _deployment.EnvironmentId = environmentId;
        return this;
    }

    public DeploymentTestBuilder WithDeployedAt(DateTime deployedAt)
    {
        _deployment.DeployedAt = deployedAt;
        return this;
    }

    public Deployment Build()
    {
        var builtDeployment = _deployment;
        _deployment = new Deployment();
        return builtDeployment;
    }
}