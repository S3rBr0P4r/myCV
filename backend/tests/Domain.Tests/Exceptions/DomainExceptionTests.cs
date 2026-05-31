using Backend.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Domain.Exceptions;

public sealed class DomainExceptionTests
{
    [Fact]
    public void NotFoundException_ShouldContainEntityNameAndKey()
    {
        var exception = new NotFoundException("CV", 42);

        exception.EntityName.Should().Be("CV");
        exception.Key.Should().Be(42);
        exception.Message.Should().Contain("CV");
        exception.Message.Should().Contain("42");
    }

    [Fact]
    public void DomainException_ShouldWrapInnerException()
    {
        var inner = new InvalidOperationException("inner error");
        var exception = new DomainException("outer error", inner);

        exception.Message.Should().Be("outer error");
        exception.InnerException.Should().BeSameAs(inner);
    }
}
