using System.Net;
using Backend.Api.Middleware;
using Backend.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Api.Middleware;

public sealed class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;
    private readonly List<string> _logMessages;

    public GlobalExceptionHandlerTests()
    {
        _logMessages = [];
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        loggerMock.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback((LogLevel level, EventId id, object state, Exception? ex, Delegate _) =>
            {
                _logMessages.Add($"{level}: {state}");
            });

        _handler = new GlobalExceptionHandler(loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_CvSourceClientException_ShouldReturn500()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var act = () => _handler.InvokeAsync(context, _ => throw new CvSourceClientException());

        await act.Should().NotThrowAsync();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("CV Source Error");
        body.Should().Contain("CV data source is currently unavailable");
    }

    [Fact]
    public async Task InvokeAsync_DomainException_ShouldReturn400()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var act = () => _handler.InvokeAsync(context, _ => throw new DomainException("Domain error"));

        await act.Should().NotThrowAsync();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("Domain Error");
        body.Should().Contain("Domain error");
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_ShouldReturn404()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var act = () => _handler.InvokeAsync(context, _ => throw new NotFoundException("CV", 1));

        await act.Should().NotThrowAsync();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("Resource Not Found");
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_ShouldReturn500()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var act = () => _handler.InvokeAsync(context, _ => throw new InvalidOperationException("Unexpected"));

        await act.Should().NotThrowAsync();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("Internal Server Error");
    }
}
