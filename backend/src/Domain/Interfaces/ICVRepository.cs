using Backend.Domain.Entities;

namespace Backend.Domain.Interfaces;

public interface ICVRepository
{
    Task<CV> GetCVAsync(string? culture = null, CancellationToken cancellationToken = default);
}
