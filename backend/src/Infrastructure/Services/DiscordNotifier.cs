using Backend.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Services;

public sealed class DiscordNotifier
{
    private readonly HttpClient _httpClient;
    private readonly DiscordOptions _options;
    private static readonly object _alertLock = new();
    private static bool _alertSent;

    public DiscordNotifier(HttpClient httpClient, IOptions<DiscordOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public Task SendAlertAsync(string title, string message)
    {
        if (_alertSent)
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrEmpty(_options.WebhookUrl))
        {
            return Task.CompletedTask;
        }

        bool shouldSend;
        lock (_alertLock)
        {
            if (_alertSent)
            {
                return Task.CompletedTask;
            }

            _alertSent = true;
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

        await _httpClient.PostAsJsonAsync(_options.WebhookUrl, payload);
    }
}
