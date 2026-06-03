namespace Backend.Domain.Entities;

public sealed class ContactInfo
{
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string WillingnessToTravel { get; init; } = string.Empty;
}
