---
name: moq-testing
description: "Unit testing patterns for .NET Clean Architecture using Moq. Covers handler-level tests with mocked repositories and direct repository tests using an in-memory database."
version: 1.0.0
language: C#
framework: .NET 10+
dependencies: Moq, xUnit, FluentAssertions
---

# Moq Testing for .NET Clean Architecture

## Overview

Unit testing patterns for this project's CQRS-lite architecture. Tests use **Moq** for mocking and **xUnit** + **FluentAssertions** for assertions.

## Project Conventions

- **xUnit** - Test framework
- **Moq** - Mocking library (not NSubstitute)
- **FluentAssertions** - Assertion library
- **No MediatR** - Direct `IRequestHandler<TQuery, TResult>` pattern
- **`_camelCase`** - Private field naming
- **`Async` suffix** - Async methods
- **Test naming**: `{Method}_{Scenario}_Should{Expected}`
- **Arrange/Act/Assert** - Three-section test structure

## Setup: Test Project csproj

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="FluentAssertions" Version="7.*" />
    <PackageReference Include="Moq" Version="4.*" />
</ItemGroup>
```

---

## 1. Handler-Level Tests

### Pattern: Query Handler with Mocked Repository

Mock the repository interface and verify the handler returns the expected result.

```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace MyCV.Application.UnitTests.CV;

public class GetCVQueryHandlerTests
{
    private readonly Mock<ICVRepository> _cvRepositoryMock;
    private readonly GetCVQueryHandler _handler;

    public GetCVQueryHandlerTests()
    {
        _cvRepositoryMock = new Mock<ICVRepository>();
        _handler = new GetCVQueryHandler(_cvRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_CVExists_ShouldReturnCVResponse()
    {
        // Arrange
        var cvId = Guid.NewGuid();
        var cv = CV.Create("John Doe", "Senior Developer", DateTime.UtcNow).Value;

        _cvRepositoryMock
            .Setup(r => r.GetByIdAsync(cvId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cv);

        var query = new GetCVQuery(cvId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("John Doe");

        _cvRepositoryMock.Verify(
            r => r.GetByIdAsync(cvId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CVNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var cvId = Guid.NewGuid();

        _cvRepositoryMock
            .Setup(r => r.GetByIdAsync(cvId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CV?)null);

        var query = new GetCVQuery(cvId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CVErrors.NotFound);
    }
}
```

### Pattern: Command Handler with Mocked Repository + UnitOfWork

```csharp
public class CreateCVCommandHandlerTests
{
    private readonly Mock<ICVRepository> _cvRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateCVCommandHandler _handler;

    public CreateCVCommandHandlerTests()
    {
        _cvRepositoryMock = new Mock<ICVRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateCVCommandHandler(
            _cvRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateAndSave()
    {
        // Arrange
        var command = new CreateCVCommand("Jane Doe", "Developer");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _cvRepositoryMock.Verify(r => r.Add(It.IsAny<CV>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var command = new CreateCVCommand("", "Developer");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _cvRepositoryMock.Verify(r => r.Add(It.IsAny<CV>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

### Pattern: Testing Domain Entity Methods

```csharp
public class CVTests
{
    [Fact]
    public void Create_ValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var name = "John Doe";
        var title = "Senior Developer";

        // Act
        var result = CV.Create(name, title, DateTime.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Title.Should().Be(title);
    }

    [Fact]
    public void Create_EmptyName_ShouldReturnError()
    {
        // Arrange
        var name = "";
        var title = "Developer";

        // Act
        var result = CV.Create(name, title, DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CVErrors.NameIsRequired);
    }

    [Fact]
    public void Update_NameChanged_ShouldRaiseDomainEvent()
    {
        // Arrange
        var cv = CV.Create("John", "Dev", DateTime.UtcNow).Value;

        // Act
        var result = cv.Update("John Updated", "Senior Dev", DateTime.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cv.Name.Should().Be("John Updated");
        cv.GetDomainEvents().Should().ContainItemsAssignableTo<CVUpdatedDomainEvent>();
    }
}
```

---

## 2. Repository Tests

### Pattern: Repository with InMemory Database (for EF Core repos)

Use EF Core's `InMemoryDatabase` or `Sqlite` for testing repository implementations directly.

```csharp
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace MyCV.Infrastructure.IntegrationTests.Repositories;

public class CVRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CVRepository _repository;

    public CVRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new CVRepository(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_CVExists_ShouldReturnCV()
    {
        // Arrange
        var cv = CV.Create("John Doe", "Developer", DateTime.UtcNow).Value;
        _dbContext.Set<CV>().Add(cv);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(cv.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByIdAsync_CVNotFound_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Add_PersistsEntity()
    {
        // Arrange
        var cv = CV.Create("Jane Doe", "Developer", DateTime.UtcNow).Value;

        // Act
        _repository.Add(cv);
        await _dbContext.SaveChangesAsync();

        // Assert
        var saved = await _dbContext.Set<CV>().FirstOrDefaultAsync(e => e.Id == cv.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task Remove_DeletesEntity()
    {
        // Arrange
        var cv = CV.Create("John", "Developer", DateTime.UtcNow).Value;
        _dbContext.Set<CV>().Add(cv);
        await _dbContext.SaveChangesAsync();

        // Act
        _repository.Remove(cv);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deleted = await _dbContext.Set<CV>().FirstOrDefaultAsync(e => e.Id == cv.Id);
        deleted.Should().BeNull();
    }
}
```

### Pattern: Repository with SQLite (more realistic than InMemory)

```csharp
public class CVRepositorySqliteTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly CVRepository _repository;

    public CVRepositorySqliteTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

        _repository = new CVRepository(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
```

---

## 3. Mock Setup Patterns

### Basic Setup

```csharp
// Return a value
mock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(entity);

// Return null (not found)
mock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync((Entity?)null);

// Void method (no setup needed for Add/Update/Remove with Moq)
// Moq doesn't require setup for void methods
```

### Argument Matching

```csharp
// Any value
It.IsAny<Guid>()
It.IsAny<CancellationToken>()

// Specific value
It.Is<Guid>(id => id == expectedId)

// Predicate
It.Is<string>(name => !string.IsNullOrEmpty(name))
```

### Verification

```csharp
// Called exactly once
mock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);

// Never called
mock.Verify(r => r.Add(It.IsAny<CV>()), Times.Never);

// Called at least once
mock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);

// No other calls were made
mock.VerifyNoOtherCalls();
```

### Strict Mocks (fail on unexpected calls)

```csharp
var mock = new Mock<ICVRepository>(MockBehavior.Strict);
mock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(cv);
// Any call not set up will throw MockException
```

---

## 4. Test Data Builders

For complex test data, use a builder pattern:

```csharp
public class CVBuilder
{
    private string _name = "Default Name";
    private string _title = "Default Title";
    private DateTime _createdAt = DateTime.UtcNow;
    private List<Experience> _experiences = new();

    public CVBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CVBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CVBuilder WithExperience(Experience experience)
    {
        _experiences.Add(experience);
        return this;
    }

    public CV Build()
    {
        var cv = CV.Create(_name, _title, _createdAt).Value;
        foreach (var exp in _experiences)
            cv.AddExperience(exp);
        return cv;
    }
}

// Usage
var cv = new CVBuilder()
    .WithName("John Doe")
    .WithTitle("Senior Developer")
    .WithExperience(Experience.Create("Company A", "Developer", 2020, 2022).Value)
    .Build();
```

---

## Project-Specific Examples

### Handler: GetCVHandler

```csharp
[Fact]
public async Task Handle_ExistingCV_ShouldMapToResponse()
{
    var cvId = Guid.NewGuid();
    var cv = CV.Create("John Doe", "Developer", DateTime.UtcNow).Value;

    _cvRepositoryMock
        .Setup(r => r.GetByIdAsync(cvId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(cv);

    var result = await _handler.Handle(new GetCVQuery(cvId), CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEquivalentTo(new CVResponse(cv.Id, cv.Name, cv.Title));
}
```

### Handler: Mapping Extensions

```csharp
[Fact]
public void ToResponse_ShouldMapAllProperties()
{
    var cv = new CVBuilder().Build();

    var response = cv.ToResponse();

    response.Id.Should().Be(cv.Id);
    response.Name.Should().Be(cv.Name);
    response.Title.Should().Be(cv.Title);
}
```

---

## 5. External Source Tests (WordCvSource)

### Pattern: Testing ICvSource Implementation with .docx Fixture

Create a valid `.docx` programmatically using `DocumentFormat.OpenXml`, then verify the `CV` entity is correctly populated.

```csharp
using Backend.Domain.Exceptions;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using Backend.Infrastructure.Sources;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public sealed class WordCvSourceTests : IDisposable
{
    private readonly string _tempDir;

    // Create a temp directory per test class for isolation
    public WordCvSourceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "mycv_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task GetCvAsync_ValidDocument_ShouldReturnCVWithAllProperties()
    {
        var filePath = CreateValidDocument();
        var sut = CreateSut(filePath);

        var cv = await sut.GetCvAsync();

        cv.Name.Should().Be("John");
        cv.LastName.Should().Be("Doe");
        cv.Title.Should().Be("Creative Developer");
        cv.Summary.Should().Be("A summary.");
        cv.Experiences.Should().HaveCount(2);
        cv.Experiences[0].Role.Should().Be("Senior Developer");
        cv.Experiences[0].Company.Should().Be("Acme Corp");
        cv.Skills.Should().BeEquivalentTo(["C#", ".NET"]);
    }

    [Fact]
    public async Task GetCvAsync_FilePathEmpty_ShouldThrowCvSourceException()
    {
        var options = Options.Create(new CvSourceOptions { FilePath = string.Empty });
        var notifier = CreateNoopNotifier();
        var sut = new WordCvSource(options, Mock.Of<ILogger<WordCvSource>>(), notifier);

        var act = () => sut.GetCvAsync();

        await act.Should().ThrowAsync<CvSourceException>()
            .WithMessage("CvSource:FilePath is not configured.");
    }

    [Fact]
    public async Task GetCvAsync_FileNotFound_ShouldThrowCvSourceException()
    {
        var sut = CreateSut("nonexistent.docx");

        var act = () => sut.GetCvAsync();

        await act.Should().ThrowAsync<CvSourceException>()
            .WithMessage("*not found*");
    }
```

### Key points for WordCvSource tests:
- **Document creation**: Uses `WordprocessingDocument.Create()` to build a minimal `.docx` in a temp file. The `AddMainDocumentPart()` only creates the part — the `Document` property is null until explicitly assigned: `mainPart.Document = new Document()`.
- **Noop DiscordNotifier**: Create with empty `WebhookUrl` so `SendAlertAsync` returns immediately without HTTP calls. No mocking required.
- **Temp directory**: `IDisposable` pattern with `_tempDir` ensures cleanup. Use `Guid` in folder name to avoid collisions.
- **SUT creation helper**: Extract `CreateSut(path)` to avoid repeating the options/logger/notifier setup.
- **Caching**: Verify `GetCvAsync_CalledTwice_ShouldReturnCachedResult` by asserting `second.Should().BeSameAs(first)`.
- **Malformed document**: Write a plain text file with `.docx` extension — `WordprocessingDocument.Open` throws, caught by the `catch (Exception ex)` in `WordCvSource.Parse()`.

---

## 6. DiscordNotifier Tests

### Pattern: Testing Static Flag with HttpMessageHandler Mock

Use `Mock<HttpMessageHandler>` with `Protected().Setup()` to capture or verify HTTP calls. Reset the static `_alertSent` flag in the constructor to avoid cross-test pollution.

```csharp
using System.Net;
using System.Reflection;
using Backend.Infrastructure.Options;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

public sealed class DiscordNotifierTests
{
    // Reset static flag between tests to avoid pollution (reflection only — no production code exposed for testing)
    public DiscordNotifierTests()
    {
        var field = typeof(DiscordNotifier).GetField("_alertSent", BindingFlags.Static | BindingFlags.NonPublic);
        field!.SetValue(null, false);
    }

    [Fact]
    public async Task SendAlertAsync_EmptyWebhookUrl_ShouldNotCallHttpClient()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new DiscordOptions { WebhookUrl = string.Empty });
        var notifier = new DiscordNotifier(httpClient, options);

        await notifier.SendAlertAsync("title", "message");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAlertAsync_CalledTwice_ShouldSendOnlyOnce()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new DiscordOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test"
        });
        var notifier = new DiscordNotifier(httpClient, options);

        await notifier.SendAlertAsync("First", "First message");
        await notifier.SendAlertAsync("Second", "Second message");

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAlertAsync_WithValidUrl_ShouldSendEmbed()
    {
        HttpRequestMessage? capturedRequest = null;
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                capturedRequest = request;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new DiscordOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test"
        });
        var notifier = new DiscordNotifier(httpClient, options);

        await notifier.SendAlertAsync("Test Title", "Test description");

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        var body = await capturedRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("Test Title");
    }
}
```

### Key points for DiscordNotifier tests:
- **Static flag reset**: The `_alertSent` field is `static` — it persists across all tests in the process. Use reflection to reset it in the constructor (`typeof(DiscordNotifier).GetField("_alertSent", BindingFlags.Static | BindingFlags.NonPublic)`). Do NOT expose `internal` methods just for testing.
- **HttpMessageHandler**: Moq's `Protected().Setup("SendAsync", ...)` mocks the protected `SendAsync` method. Verify with `Protected().Verify("SendAsync", Times.Once(), ...)`.
- **Request capture**: Use a callback in `ReturnsAsync` to capture the `HttpRequestMessage` for body/content assertions.
- **Noop**: Empty `WebhookUrl` causes `DiscordNotifier` to return immediately without any HTTP call.
- **Moq.Protected namespace**: Requires `using Moq.Protected;`.

---

## 7. Middleware Tests (GlobalExceptionHandler)

### Pattern: Testing Middleware with DefaultHttpContext

Use `DefaultHttpContext` to simulate an HTTP request/response pipeline, and pass an exception-throwing `RequestDelegate` to the middleware.

```csharp
using System.Net;
using Backend.Api.Middleware;
using Backend.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public sealed class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_CvSourceException_ShouldReturn500()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await _handler.InvokeAsync(context,
            _ => throw new CvSourceException("CV source failed"));

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("CV Source Error");
    }

    [Fact]
    public async Task InvokeAsync_DomainException_ShouldReturn400()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await _handler.InvokeAsync(context,
            _ => throw new DomainException("Domain error"));

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
}
```

### Key points for GlobalExceptionHandler tests:
- **DefaultHttpContext**: Part of `Microsoft.AspNetCore.Http` (available via implicit usings in Web SDK projects; add explicit using in test projects without it).
- **Exception-throwing delegate**: Pass `(RequestDelegate)(_ => throw new Exception(...))` as the `next` parameter.
- **Response body**: Set `context.Response.Body = new MemoryStream()` to capture response output for content assertions.
- **Seek before read**: Call `context.Response.Body.Seek(0, SeekOrigin.Begin)` before reading the body stream.
- **Log capturing (optional)**: Use a `Mock<ILogger<T>>` callback on the `Log` method to capture log messages if you need to assert log output.

---

## Best Practices

1. **Tests cover only public APIs** - Never write tests that target `private` or `internal` methods directly. All test scenarios must exercise the SUT exclusively through its public methods. Private methods are implementation details — if they need testing, refactor them into separate public types. Never expose `internal` members (`InternalsVisibleTo`, test-only methods) solely for testing purposes.
2. **One mock per test concern** - Don't mock everything in one test
3. **Test behavior, not implementation** - Verify what, not how
4. **Use `Times` enum** - Be explicit about call counts
5. **Avoid `VerifyAll`** - Prefer explicit `Verify` calls
6. **Keep setup in constructor** - Use constructor for common mocks
7. **Name tests clearly** - `{Method}_{Scenario}_Should{Expected}`
8. **InMemory for unit, SQLite for integration** - Match fidelity to need
9. **Dispose DbContext** - Implement `IDisposable` in repository test classes
10. **Test failures too** - Test missing entities, validation errors, edge cases
11. **`MockBehavior.Strict` sparingly** - Use only when you need to ensure no extra calls
