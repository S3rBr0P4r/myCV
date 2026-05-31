using Backend.Application.DTOs;

namespace Backend.Application.UseCases.GetCV;

public sealed record GetCVQuery(string? Culture = null);

public sealed record GetCVResult(CVDto CV);
