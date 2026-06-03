namespace Backend.Domain.Entities;

public sealed class Certification
{
    public required string Category { get; init; }
    public required string Title { get; init; }
    public string Issuer { get; init; } = string.Empty;
}
