using Backend.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Infrastructure.Persistence;

public sealed class CVRepositoryTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenSourceIsNull()
    {
        var act = () => new CVRepository(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }
}
