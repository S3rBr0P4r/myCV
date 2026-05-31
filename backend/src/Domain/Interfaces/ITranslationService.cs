using Backend.Domain.Entities;

namespace Backend.Domain.Interfaces;

public interface ITranslationService
{
    Task<CV?> TranslateAsync(CV source, string targetLanguage, CancellationToken cancellationToken = default);
}
