using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Backend.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _docxDirectory;
    private readonly string _docxFilePath;
    private bool _createDocx = true;

    public string DocxFilePath => _docxFilePath;

    public CustomWebApplicationFactory()
    {
        _docxDirectory = Path.Combine(Path.GetTempPath(), "mycv_int_" + Guid.NewGuid().ToString("N"));
        _docxFilePath = Path.Combine(_docxDirectory, "cv.docx");
    }

    public void SkipDocxCreation()
    {
        _createDocx = false;
    }

    public async Task InitializeAsync()
    {
        if (!_createDocx)
        {
            return;
        }

        Directory.CreateDirectory(_docxDirectory);
        await TestDocumentFixture.CreateValidDocumentAsync(_docxFilePath);
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (Directory.Exists(_docxDirectory))
        {
            Directory.Delete(_docxDirectory, recursive: true);
        }

        base.Dispose(disposing);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseUrls("http://127.0.0.1:0");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CvSource:FilePath"] = _docxFilePath,
                ["CvSource:AllowedDirectory"] = "",
                ["DeepL:AuthKey"] = "",
                ["Discord:ErrorWebhookUrl"] = "",
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173",
                ["Cors:FrontendUrl"] = "http://localhost:5173",
                ["Serilog:MinimumLevel:Default"] = "Fatal",
            }!);
        });
    }
}
