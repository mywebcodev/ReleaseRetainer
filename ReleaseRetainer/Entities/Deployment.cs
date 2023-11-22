namespace ReleaseRetainer.Entities;

public record Deployment
{
    public required string Id { get; set; }
    public required string ReleaseId { get; set; }
    public required string EnvironmentId { get; set; }
    public required DateTime DeployedAt { get; set; }
}