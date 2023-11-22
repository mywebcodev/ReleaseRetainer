namespace ReleaseRetainer.Entities;

public record Environment
{
    public required string Id { get; set; }
    public required string Name { get; set; }
}