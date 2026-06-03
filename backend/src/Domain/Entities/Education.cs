namespace Backend.Domain.Entities;

public sealed class Education
{
    public required string Degree { get; init; }
    public required string Institution { get; init; }
    public string Notes { get; init; } = string.Empty;
}
