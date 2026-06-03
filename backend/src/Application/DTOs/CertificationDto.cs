namespace Backend.Application.DTOs;

public sealed record CertificationDto(
    string Category,
    string Title,
    string Issuer);
