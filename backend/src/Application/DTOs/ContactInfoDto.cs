namespace Backend.Application.DTOs;

public sealed record ContactInfoDto(
    string Email,
    string Phone,
    string Location,
    string WillingnessToTravel);
