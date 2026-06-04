using DocumentFormat.OpenXml.Wordprocessing;

namespace Backend.Infrastructure.Sources;

internal static class TextFormatter
{
    internal static string GetFormattedText(Paragraph paragraph, Dictionary<string, Uri>? hyperlinkRels = null)
    {
        var raw = paragraph.InnerText.Trim();
        if (raw.Length == 0)
        {
            return raw;
        }

        var lower = raw.ToLowerInvariant();
        if (SectionHelper.SectionHeaders.Contains(lower))
        {
            return raw;
        }

        var parts = new List<string>();

        foreach (var child in paragraph.ChildElements)
        {
            if (child is Run run)
            {
                var text = run.InnerText;
                if (text.Length == 0)
                {
                    continue;
                }

                var isBold = run.RunProperties?.Bold is not null;
                parts.Add(isBold ? $"**{text}**" : text);
            }
            else if (child is Hyperlink hyperlink && hyperlinkRels is not null)
            {
                var relId = hyperlink.Id?.Value;
                if (relId is null || !hyperlinkRels.TryGetValue(relId, out var url))
                {
                    continue;
                }

                var linkParts = hyperlink.Elements<Run>()
                    .Select(r =>
                    {
                        var text = r.InnerText;
                        if (text.Length == 0)
                        {
                            return text;
                        }

                        var isBold = r.RunProperties?.Bold is not null;
                        return isBold ? $"**{text}**" : text;
                    });

                var linkText = string.Concat(linkParts).Trim();
                if (linkText.Length > 0)
                {
                    parts.Add($"[{linkText}]({url})");
                }
            }
        }

        return string.Concat(parts).Trim();
    }
}
