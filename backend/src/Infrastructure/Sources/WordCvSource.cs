using Backend.Domain.Entities;
using Backend.Domain.Exceptions;
using Backend.Domain.Interfaces;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Sources;

public sealed class WordCvSource : ICvSource
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly string _filePath;
    private readonly string _allowedDirectory;
    private readonly ILogger<WordCvSource> _logger;
    private readonly DiscordNotifier _discordNotifier;
    private CV? _cv;
    private bool _parsed;
    private readonly object _parseLock = new();

    public WordCvSource(
        IOptions<CvSourceOptions> options,
        ILogger<WordCvSource> logger,
        DiscordNotifier discordNotifier)
    {
        _filePath = options.Value.FilePath ?? string.Empty;
        _allowedDirectory = options.Value.AllowedDirectory ?? string.Empty;
        _logger = logger;
        _discordNotifier = discordNotifier;
    }

    public Task<CV> GetCvAsync(CancellationToken cancellationToken = default)
    {
        if (_parsed)
        {
            return Task.FromResult(_cv!);
        }

        lock (_parseLock)
        {
            if (_parsed)
            {
                return Task.FromResult(_cv!);
            }

            Parse();
        }

        return Task.FromResult(_cv!);
    }

    private void Parse()
    {
        ValidateFile();

        try
        {
            var cv = CvDocumentReader.Read(_filePath);
            _cv = cv;
            _parsed = true;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully parsed CV from {FilePath}", _filePath);
            }
        }
        catch (CvSourceClientException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(ex, "Failed to parse CV Word document at {FilePath}", _filePath);
            }

            _discordNotifier.SendAlertAsync(
                "CV Parse Error",
                $"Failed to parse the CV Word document at:\n`{_filePath}`\n\n**Error:** {ex.Message}\n\nThe CV endpoint will return 500.");
            throw new CvSourceClientException();
        }
    }

    private void ValidateFile()
    {
        ValidatePathConfigured();
        ValidateNotUncPath();
        ValidateAllowedDirectory();
        ValidateFileExists();
        ValidateFileSize();
    }

    private void ValidatePathConfigured()
    {
        if (!string.IsNullOrEmpty(_filePath))
        {
            return;
        }

        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("CvSource:FilePath is not configured");
        }

        _discordNotifier.SendAlertAsync(
            "CV Source Error",
            "CvSource:FilePath is not configured. The CV endpoint will return 500.");
        throw new CvSourceClientException();
    }

    private void ValidateNotUncPath()
    {
        if (!_filePath.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return;
        }

        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("CV Source file path is a UNC path; rejecting for security: {FilePath}", _filePath);
        }

        _discordNotifier.SendAlertAsync(
            "CV Source Security",
            $"CV Source file path is a UNC path. Rejected for security.");
        throw new CvSourceClientException();
    }

    private void ValidateAllowedDirectory()
    {
        if (string.IsNullOrEmpty(_allowedDirectory))
        {
            return;
        }

        var fullPath = Path.GetFullPath(_filePath);
        var allowedDir = Path.GetFullPath(_allowedDirectory);
        if (fullPath.StartsWith(allowedDir, StringComparison.Ordinal))
        {
            return;
        }

        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("CV Source file path is outside the allowed directory");
        }

        _discordNotifier.SendAlertAsync(
            "CV Source Security",
            "CV Source file path is outside the allowed directory.");
        throw new CvSourceClientException();
    }

    private void ValidateFileExists()
    {
        var fullPath = Path.GetFullPath(_filePath);
        if (File.Exists(fullPath))
        {
            return;
        }

        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("CV Word document not found at {FilePath}", _filePath);
        }

        _discordNotifier.SendAlertAsync(
            "CV File Not Found",
            $"The CV Word document was not found at:\n`{_filePath}`\n\nThe CV endpoint will return 500.");
        throw new CvSourceClientException();
    }

    private void ValidateFileSize()
    {
        var fullPath = Path.GetFullPath(_filePath);
        var fileInfo = new FileInfo(fullPath);
        if (fileInfo.Length <= MaxFileSize)
        {
            return;
        }

        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("CV Word document exceeds maximum size ({Size} bytes > {Max} bytes)", fileInfo.Length, MaxFileSize);
        }

        _discordNotifier.SendAlertAsync(
            "CV File Too Large",
            $"The CV Word document exceeds the maximum file size of {MaxFileSize / 1024 / 1024} MB.");
        throw new CvSourceClientException();
    }
}
