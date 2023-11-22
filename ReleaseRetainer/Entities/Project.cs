namespace ReleaseRetainer.Entities;

public record Project
{
    public required string Id { get; set; }
    public required string Name { get; set; }
}