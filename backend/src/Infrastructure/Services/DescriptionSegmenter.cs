using System.Text;

namespace Backend.Infrastructure.Services;

public static class DescriptionSegmenter
{
    private static readonly string[] ProtectedMarkers = ["Tech Stack", "Core", "Tooling"];

    public static IEnumerable<string> CollectTranslatable(string description)
    {
        return SplitLines(description)
            .SelectMany(line => line)
            .Where(segment => !segment.IsProtected && segment.Text.Length > 0)
            .Select(segment => segment.Text);
    }

    public static string Rebuild(string description, Func<string, string> translate)
    {
        var rebuiltLines = new List<string>();
        foreach (var line in SplitLines(description))
        {
            var builder = new StringBuilder();
            foreach (var segment in line)
            {
                builder.Append(segment.IsProtected || segment.Text.Length == 0
                    ? segment.Text
                    : translate(segment.Text));
            }
            rebuiltLines.Add(builder.ToString());
        }
        return string.Join("\n", rebuiltLines);
    }

    private static List<List<(bool IsProtected, string Text)>> SplitLines(string description)
    {
        return description.Split('\n').Select(SplitLine).ToList();
    }

    private static List<(bool IsProtected, string Text)> SplitLine(string line)
    {
        var payloadStart = FindPayloadStart(line);
        if (payloadStart < 0 || payloadStart >= line.Length)
        {
            return [(false, line)];
        }

        return [(false, line[..payloadStart]), (true, line[payloadStart..])];
    }

    private static int FindPayloadStart(string line)
    {
        var prefix = line.StartsWith("**", StringComparison.Ordinal) ? 2 : 0;

        foreach (var marker in ProtectedMarkers)
        {
            if (!line.AsSpan(prefix).StartsWith(marker, StringComparison.Ordinal))
            {
                continue;
            }

            var markerEnd = prefix + marker.Length;
            var colonIdx = line.IndexOf(':', markerEnd);
            if (colonIdx < 0)
            {
                return -1;
            }

            if (!line.AsSpan(markerEnd, colonIdx - markerEnd).Trim('*').IsEmpty)
            {
                return -1;
            }

            var payloadStart = colonIdx + 1;
            if (line.AsSpan(payloadStart).StartsWith("**", StringComparison.Ordinal))
            {
                payloadStart += 2;
            }

            return payloadStart;
        }

        return -1;
    }
}
