using Backend.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Services;

public sealed class DiscordErrorNotifier
{
    private static readonly TimeSpan AlertCooldown = TimeSpan.FromHours(1);
    private static readonly object _alertLock = new();
    private static DateTime _lastAlertTime = DateTime.MinValue;

    public static void ResetCooldown()
    {
        _lastAlertTime = DateTime.MinValue;
    }

    private readonly HttpClient _httpClient;
    private readonly DiscordOptions _options;
    private readonly ILogger<DiscordErrorNotifier> _logger;

    public DiscordErrorNotifier(HttpClient httpClient, IOptions<DiscordOptions> options, ILogger<DiscordErrorNotifier> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task SendAlertAsync(string title, string message)
    {
        var now = DateTime.UtcNow;
        if (now - _lastAlertTime < AlertCooldown)
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrEmpty(_options.ErrorWebhookUrl))
        {
            return Task.CompletedTask;
        }

        if (!Uri.TryCreate(_options.ErrorWebhookUrl, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
        {
            return Task.CompletedTask;
        }

        bool shouldSend;
        lock (_alertLock)
        {
            if (now - _lastAlertTime < AlertCooldown)
            {
                return Task.CompletedTask;
            }

            _lastAlertTime = now;
            shouldSend = true;
        }

        if (shouldSend)
        {
            return SendAsync(title, message);
        }

        return Task.CompletedTask;
    }

    private async Task SendAsync(string title, string message)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        var payload = new
        {
            embeds = new[]
            {
                new
                {
                    title,
                    description = message,
                    color = 0xFF0000,
                    timestamp = DateTimeOffset.UtcNow.ToString("o")
                }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_options.ErrorWebhookUrl, payload, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogWarning("Discord webhook returned {StatusCode}: {Body}", (int)response.StatusCode, body);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Discord webhook request timed out.");
        }
    }
}
