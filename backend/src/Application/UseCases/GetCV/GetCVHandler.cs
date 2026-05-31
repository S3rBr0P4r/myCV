using Backend.Application.Mappings;
using Backend.Domain.Interfaces;

namespace Backend.Application.UseCases.GetCV;

public sealed class GetCVHandler
{
    private readonly ICVRepository _cvRepository;

    public GetCVHandler(ICVRepository cvRepository)
    {
        _cvRepository = cvRepository ?? throw new ArgumentNullException(nameof(cvRepository));
    }

    public async Task<GetCVResult> HandleAsync(GetCVQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var cv = await _cvRepository.GetCVAsync(query.Culture, cancellationToken);
        var dto = cv.ToDto();

        return new GetCVResult(dto);
    }
}
