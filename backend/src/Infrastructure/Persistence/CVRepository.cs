using Backend.Domain.Entities;
using Backend.Domain.Interfaces;

namespace Backend.Infrastructure.Persistence;

public sealed class CVRepository : ICVRepository
{
    private readonly ICvSource _source;

    public CVRepository(ICvSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public async Task<CV> GetCVAsync(string? culture = null, CancellationToken cancellationToken = default)
    {
        return await _source.GetCvAsync(cancellationToken);
    }
}
