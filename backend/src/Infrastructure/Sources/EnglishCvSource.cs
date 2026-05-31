using Backend.Domain.Entities;
using Backend.Domain.Interfaces;

namespace Backend.Infrastructure.Sources;

public sealed class EnglishCvSource : ICvSource
{
    public Task<CV> GetCvAsync(CancellationToken cancellationToken = default)
    {
        var cv = new CV
        {
            Name = "John",
            LastName = "Doe",
            Title = "Creative Developer & Architect",
            Summary = "Building digital experiences with the softness of a sunset and the precision of a craftsman.",
            Experiences =
            [
                new Experience
                {
                    Period = "2024 - PRESENT",
                    Role = "Senior Developer",
                    Company = "TECH NOIR SYSTEMS",
                    Description = "Redefining the web with handcrafted, fluid architecture.",
                    Background = "bg-placeholder-1"
                },
                new Experience
                {
                    Period = "2021 - 2023",
                    Role = "Full Stack Engineer",
                    Company = "NEON DIGITAL",
                    Description = "Creating immersive worlds with attention to detail.",
                    Background = "bg-placeholder-2"
                }
            ],
            Skills = ["C#", ".NET 10", "TypeScript", "Clean Architecture", "Ghibli Design"]
        };

        return Task.FromResult(cv);
    }
}
