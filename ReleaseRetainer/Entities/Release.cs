namespace ReleaseRetainer.Entities;

public class Release
{
    public required string Id { get; set; }
    public string? ProjectId { get; set; }
    public string? Version { get; set; }
    public required DateTime Created { get; set; }
}