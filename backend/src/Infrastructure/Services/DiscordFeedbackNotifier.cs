using Backend.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Services;

public sealed class DiscordFeedbackNotifier
{
    private readonly HttpClient _httpClient;
    private readonly DiscordOptions _options;
    private readonly ILogger<DiscordFeedbackNotifier> _logger;

    public DiscordFeedbackNotifier(HttpClient httpClient, IOptions<DiscordOptions> options, ILogger<DiscordFeedbackNotifier> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task SendAsync(string country, int rating, string name, string comment = "")
    {
        if (string.IsNullOrEmpty(_options.FeedbackWebhookUrl))
        {
            return Task.CompletedTask;
        }

        if (!Uri.TryCreate(_options.FeedbackWebhookUrl, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
        {
            return Task.CompletedTask;
        }

        return SendPayloadAsync(country, rating, name, comment);
    }

    private async Task SendPayloadAsync(string country, int rating, string name, string comment)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        var stars = new string('\u2B50', rating);
        var fields = new List<object>
        {
            new { name = "Name", value = name, inline = true },
            new { name = "Country", value = country, inline = true },
            new { name = "Rating", value = $"{stars} ({rating}/5)", inline = false }
        };

        if (!string.IsNullOrWhiteSpace(comment))
        {
            fields.Add(new { name = "Comment", value = comment, inline = false });
        }

        var payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = "New Feedback",
                    color = 0x2ECC71,
                    fields,
                    timestamp = DateTimeOffset.UtcNow.ToString("o")
                }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_options.FeedbackWebhookUrl, payload, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogWarning("Feedback webhook returned {StatusCode}: {Body}", (int)response.StatusCode, body);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Feedback webhook request timed out.");
        }
    }
}
