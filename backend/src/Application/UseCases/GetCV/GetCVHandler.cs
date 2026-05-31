using Backend.Application.Mappings;
using Backend.Domain.Interfaces;

namespace Backend.Application.UseCases.GetCV;

public sealed class GetCVHandler
{
    private readonly ICVRepository _cvRepository;
    private readonly ITranslationService _translationService;

    public GetCVHandler(ICVRepository cvRepository, ITranslationService translationService)
    {
        _cvRepository = cvRepository ?? throw new ArgumentNullException(nameof(cvRepository));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
    }

    public async Task<GetCVResult> HandleAsync(GetCVQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var cv = await _cvRepository.GetCVAsync(query.Culture, cancellationToken);

        var translated = await _translationService.TranslateAsync(cv, query.Culture ?? string.Empty, cancellationToken);
        var result = translated ?? cv;

        var dto = result.ToDto();

        return new GetCVResult(dto);
    }
}
