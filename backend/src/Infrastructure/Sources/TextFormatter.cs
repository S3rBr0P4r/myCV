using System.Text;
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
        var pending = new StringBuilder();
        bool? pendingBold = null;

        foreach (var child in paragraph.ChildElements)
        {
            if (child is Run run)
            {
                AppendRun(ref pending, ref pendingBold, parts, run);
            }
            else if (child is Hyperlink hyperlink && hyperlinkRels is not null)
            {
                FlushPending(ref pending, ref pendingBold, parts);
                AppendHyperlink(hyperlink, hyperlinkRels, parts);
            }
        }

        FlushPending(ref pending, ref pendingBold, parts);

        return string.Concat(parts).Trim();
    }

    private static void AppendRun(
        ref StringBuilder pending,
        ref bool? pendingBold,
        List<string> parts,
        Run run)
    {
        var text = run.InnerText;
        if (text.Length == 0)
        {
            return;
        }

        var isBold = run.RunProperties?.Bold is not null;

        if (pendingBold.HasValue && pendingBold.Value != isBold)
        {
            FlushPending(ref pending, ref pendingBold, parts);
        }

        pendingBold = isBold;
        pending.Append(text);
    }

    private static void FlushPending(
        ref StringBuilder pending,
        ref bool? pendingBold,
        List<string> parts)
    {
        if (!pendingBold.HasValue || pending.Length == 0)
        {
            pending.Clear();
            pendingBold = null;
            return;
        }

        parts.Add(pendingBold.Value ? $"**{pending}**" : pending.ToString());
        pending.Clear();
        pendingBold = null;
    }

    private static void AppendHyperlink(
        Hyperlink hyperlink,
        Dictionary<string, Uri> hyperlinkRels,
        List<string> parts)
    {
        var relId = hyperlink.Id?.Value;
        if (relId is null || !hyperlinkRels.TryGetValue(relId, out var url))
        {
            return;
        }

        var linkText = MergedRunText(hyperlink.Elements<Run>()).Trim();
        if (linkText.Length > 0)
        {
            parts.Add($"[{linkText}]({url})");
        }
    }

    private static string MergedRunText(IEnumerable<Run> runs)
    {
        var parts = new List<string>();
        var pending = new StringBuilder();
        bool? pendingBold = null;

        foreach (var run in runs)
        {
            AppendRun(ref pending, ref pendingBold, parts, run);
        }

        FlushPending(ref pending, ref pendingBold, parts);

        return string.Concat(parts);
    }
}
