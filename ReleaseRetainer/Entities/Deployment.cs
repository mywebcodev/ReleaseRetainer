namespace ReleaseRetainer.Entities;

public record Deployment
{
    public string Id { get; set; }
    public string ReleaseId { get; set; }
    public string EnvironmentId { get; set; }
    public DateTime DeployedAt { get; set; }
}