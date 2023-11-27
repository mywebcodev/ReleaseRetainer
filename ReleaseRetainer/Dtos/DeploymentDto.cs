namespace ReleaseRetainer.Dtos;

public record DeploymentDto
{
    public string Id { get; set; }
    public string ReleaseId { get; set; }
    public string EnvironmentId { get; set; }
    public DateTime DeployedAt { get; set; }
}