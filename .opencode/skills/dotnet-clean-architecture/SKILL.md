---
name: dotnet-clean-architecture
description: "Scaffolds a complete .NET solution following Clean Architecture principles with proper layer separation (API, Application, Domain, Infrastructure). Creates project structure, dependency injection setup, and cross-cutting concerns configuration."
version: 1.0.0
language: C#
framework: .NET 8+
dependencies: MediatR, FluentValidation, Entity Framework Core, Dapper
---

# .NET Clean Architecture Project Scaffolder

## Overview

This skill generates a complete .NET solution following Clean Architecture (also known as Onion Architecture or Hexagonal Architecture). The architecture enforces separation of concerns through distinct layers with unidirectional dependencies pointing inward.

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│  Controllers, Middleware, Request/Response DTOs             │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                       │
│  EF Core, Repositories, External Services, Authentication   │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                         │
│  Commands, Queries, Handlers, Validators, DTOs              │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                            │
│  Entities, Value Objects, Domain Events, Interfaces         │
└─────────────────────────────────────────────────────────────┘
```

**Dependency Rule**: Dependencies point inward. Domain has no dependencies. Application depends only on Domain. Infrastructure implements interfaces from Domain/Application.

## Quick Reference

| Task | Command/Action |
|------|----------------|
| Create solution | `dotnet new sln -n {SolutionName}` |
| Create Domain project | `dotnet new classlib -n {name}.domain` |
| Create Application project | `dotnet new classlib -n {name}.application` |
| Create Infrastructure project | `dotnet new classlib -n {name}.infrastructure` |
| Create API project | `dotnet new webapi -n {name}.api` |
| Add project to solution | `dotnet sln add src/{project}/{project}.csproj` |
| Add project reference | `dotnet add reference ../other/other.csproj` |

## Project Structure

```
{SolutionName}/
├── src/
│   ├── {name}.domain/
│   │   ├── Abstractions/
│   │   │   ├── Entity.cs
│   │   │   ├── IDomainEvent.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── Result.cs
│   │   ├── {Aggregate}/
│   │   │   ├── {Entity}.cs
│   │   │   ├── {Entity}Errors.cs
│   │   │   ├── I{Entity}Repository.cs
│   │   │   ├── ValueObjects/
│   │   │   └── Events/
│   │   └── {name}.domain.csproj
│   │
│   ├── {name}.application/
│   │   ├── Abstractions/
│   │   │   ├── Behaviors/
│   │   │   └── Messaging/
│   │   ├── {Feature}/
│   │   │   ├── Create{Entity}/
│   │   │   ├── Update{Entity}/
│   │   │   ├── Delete{Entity}/
│   │   │   └── Get{Entity}/
│   │   ├── DependencyInjection.cs
│   │   └── {name}.application.csproj
│   │
│   ├── {name}.infrastructure/
│   │   ├── Repositories/
│   │   ├── ApplicationDbContext.cs
│   │   ├── DependencyInjection.cs
│   │   └── {name}.infrastructure.csproj
│   │
│   └── {name}.api/
│       ├── Controllers/
│       ├── Middleware/
│       ├── Program.cs
│       ├── appsettings.json
│       └── {name}.api.csproj
│
└── {SolutionName}.sln
```

## Naming Conventions

| Item | Convention | Example |
|------|------------|---------|
| Solution | PascalCase | `MyCV` |
| Projects | lowercase with dots | `mycv.domain` |
| Namespaces | lowercase | `mycv.domain.cv` |
| Classes | PascalCase | `CVRepository` |
| Interfaces | IPascalCase | `ICVRepository` |
| Queries | Get{Entity}Query | `GetCVQuery` |
| Handlers | {Command/Query}Handler | `GetCVHandler` |
| Responses | {Entity}Response | `CVResponse` |
| Domain Events | {Entity}{Action}DomainEvent | `CVCreatedDomainEvent` |
| Errors | {Entity}Errors | `CVErrors` |

## Critical Rules

1. **Domain has ZERO dependencies** on other layers
2. **Application depends only on Domain** - no infrastructure concerns
3. **Infrastructure implements interfaces** defined in Domain/Application
4. **API only references Application and Infrastructure**
5. **Use Result pattern** instead of exceptions for business logic errors
6. **Commands modify state**, Queries read state (CQRS)
7. **One handler per Command/Query** - no shared handlers
8. **Repositories are per aggregate root** - not per entity
9. **Domain events are raised in domain**, handled in application layer
10. **Always use CancellationToken** in async operations
