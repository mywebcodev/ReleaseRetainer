namespace ReleaseRetainer.Entities;

public record Release
{
    public required string Id { get; set; }
    public required string ProjectId { get; set; }
    public required string Version { get; set; }
    public required DateTime Created { get; set; }
}