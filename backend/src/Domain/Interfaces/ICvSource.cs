using Backend.Domain.Entities;

namespace Backend.Domain.Interfaces;

public interface ICvSource
{
    Task<CV> GetCvAsync(CancellationToken cancellationToken = default);
}
