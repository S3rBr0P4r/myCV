using Backend.Domain.Entities;

namespace Backend.Infrastructure.Sources;

internal static class ContactParser
{
    internal static ContactInfo? ParseContactInfo(List<string> lines, int startIndex)
    {
        if (startIndex >= lines.Count)
        {
            return null;
        }

        string email = string.Empty;
        string phone = string.Empty;
        string location = string.Empty;
        string willingness = string.Empty;

        for (int i = startIndex; i < lines.Count; i++)
        {
            var line = lines[i];
            if (SectionHelper.IsSectionHeader(line))
            {
                break;
            }

            var value = StripIconPrefix(line);

            if (value.Contains('@'))
            {
                email = value;
            }
            else if (value.StartsWith('+') || value.Any(char.IsDigit))
            {
                phone = value;
            }
            else if (value.Contains("Remote", StringComparison.OrdinalIgnoreCase) ||
                     value.Contains("Hybrid", StringComparison.OrdinalIgnoreCase) ||
                     value.Contains("Onsite", StringComparison.OrdinalIgnoreCase))
            {
                location = value;
            }
            else if (value.Contains("Willingness", StringComparison.OrdinalIgnoreCase) ||
                     value.Contains("travel", StringComparison.OrdinalIgnoreCase))
            {
                willingness = value;
            }
        }

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone) &&
            string.IsNullOrEmpty(location) && string.IsNullOrEmpty(willingness))
        {
            return null;
        }

        return new ContactInfo
        {
            Email = email,
            Phone = phone,
            Location = location,
            WillingnessToTravel = willingness
        };
    }

    internal static string StripIconPrefix(string text)
    {
        if (text.Length == 0)
        {
            return text;
        }

        int skip = 0;
        if (char.IsHighSurrogate(text[0]) && text.Length > 1)
        {
            skip = 2;
        }
        else if (!char.IsLetterOrDigit(text[0]) && text[0] != ' ')
        {
            skip = 1;
        }

        if (skip > 0)
        {
            var afterIcon = text[skip..].TrimStart();
            while (afterIcon.Length > 0 && afterIcon[0] == '\uFE0F')
            {
                afterIcon = afterIcon[1..].TrimStart();
            }
            return afterIcon.Length > 0 ? afterIcon : text;
        }

        return text;
    }
}
