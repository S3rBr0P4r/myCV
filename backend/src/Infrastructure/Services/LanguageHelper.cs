using System.Globalization;

namespace Backend.Infrastructure.Services;

public static class LanguageHelper
{
    public static string? NormalizeLanguage(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return null;
        }

        var primary = culture.Split(',')[0].Trim().Split(';')[0].Trim();

        try
        {
            var ci = CultureInfo.GetCultureInfo(primary);
            return ci.TwoLetterISOLanguageName.ToUpperInvariant();
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
