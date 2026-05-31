---
name: dotnet-domain-entity-generator
description: "Generates Domain Entities following DDD principles with factory methods, private setters, domain events, and proper encapsulation. Supports aggregate roots, child entities, and value objects."
version: 1.0.0
language: C#
framework: .NET 8+
pattern: Domain-Driven Design
---

# Domain Entity Generator

## Overview

This skill generates Domain Entities following Domain-Driven Design (DDD) principles:

- **Encapsulation** - Private setters, controlled modification
- **Factory Methods** - Static `Create()` methods with validation
- **Domain Events** - State changes raise events
- **Rich Domain Model** - Behavior lives in the entity, not services
- **Invariant Protection** - Entity always in valid state

## Quick Reference

| Concept | Purpose | Example |
|---------|---------|---------|
| Aggregate Root | Entry point for aggregate | `CV`, `Skill` |
| Child Entity | Part of aggregate, no own identity outside | `Education`, `Experience` |
| Value Object | Immutable, no identity | `Email`, `PhoneNumber` |
| Domain Event | Signal state change | `CVCreatedDomainEvent` |

## Entity Structure

```
/Domain/{Aggregate}/
├── {Entity}.cs                    # Main entity
├── {Entity}Errors.cs              # Typed errors
├── I{Entity}Repository.cs         # Repository interface
├── ValueObjects/
└── Events/
```

## Template: Aggregate Root Entity

```csharp
using {name}.domain.abstractions;
using {name}.domain.{aggregate}.events;

namespace {name}.domain.{aggregate};

public sealed class {Entity} : Entity
{
    private readonly List<{ChildEntity}> _{childEntities} = new();

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<{ChildEntity}> {ChildEntities} => _{childEntities}.AsReadOnly();

    private {Entity}() { }

    private {Entity}(Guid id, string name, string? description, DateTime createdAt)
        : base(id)
    {
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static Result<{Entity}> Create(string name, string? description, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<{Entity}>({Entity}Errors.NameIsRequired);

        var {entity} = new {Entity}(Guid.NewGuid(), name, description, createdAt);
        {entity}.RaiseDomainEvent(new {Entity}CreatedDomainEvent({entity}.Id));
        return {entity};
    }

    public Result Update(string name, string? description, DateTime updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure({Entity}Errors.NameIsRequired);

        Name = name;
        Description = description;
        UpdatedAt = updatedAt;
        RaiseDomainEvent(new {Entity}UpdatedDomainEvent(Id));
        return Result.Success();
    }

    public Result Add{ChildEntity}({ChildEntity} {childEntity})
    {
        if ({childEntity} is null)
            return Result.Failure({Entity}Errors.Child{ChildEntity}Required);

        _{childEntities}.Add({childEntity});
        RaiseDomainEvent(new {ChildEntity}AddedDomainEvent(Id, {childEntity}.Id));
        return Result.Success();
    }

    public Result Remove{ChildEntity}(Guid {childEntity}Id)
    {
        var {childEntity} = _{childEntities}.FirstOrDefault(c => c.Id == {childEntity}Id);
        if ({childEntity} is null)
            return Result.Failure({Entity}Errors.{ChildEntity}NotFound);

        _{childEntities}.Remove({childEntity});
        return Result.Success();
    }
}
```

## Template: Value Object

```csharp
namespace {name}.domain.{aggregate}.valueobjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(EmailErrors.Empty);

        email = email.Trim().ToLowerInvariant();
        if (email.Length > 255)
            return Result.Failure<Email>(EmailErrors.TooLong);

        return new Email(email);
    }

    public override string ToString() => Value;
    public static implicit operator string(Email email) => email.Value;
}
```

## Template: Domain Errors

```csharp
namespace {name}.domain.{aggregate};

public static class {Entity}Errors
{
    public static readonly Error NotFound = new(
        "{Entity}.NotFound", "The {entity} with the specified ID was not found");

    public static readonly Error NameIsRequired = new(
        "{Entity}.NameRequired", "{Entity} name is required");

    public static readonly Error AlreadyExists = new(
        "{Entity}.AlreadyExists", "A {entity} with this name already exists");
}
```

## Template: Domain Events

```csharp
namespace {name}.domain.{aggregate}.events;

public sealed record {Entity}CreatedDomainEvent(Guid {Entity}Id) : IDomainEvent;
public sealed record {Entity}UpdatedDomainEvent(Guid {Entity}Id) : IDomainEvent;
```

## Template: Repository Interface

```csharp
namespace {name}.domain.{aggregate};

public interface I{Entity}Repository
{
    Task<{Entity}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add({Entity} {entity});
    void Update({Entity} {entity});
    void Remove({Entity} {entity});
}
```

## Critical DDD Rules

1. **Private setters always** - No direct property modification from outside
2. **Factory methods for creation** - `Create()` static methods with validation
3. **Domain events for state changes**
4. **Entities are always valid** - Invariants protected in constructors and methods
5. **Aggregate root controls children**
6. **Value objects are immutable** - Use `record` types
7. **Repository per aggregate root**
8. **Use Result pattern** - Return errors, don't throw
9. **Keep entities persistence-ignorant** - No EF Core attributes on domain

## Anti-Patterns to Avoid

```csharp
// ❌ WRONG: Public setters
public string Name { get; set; }

// ✅ CORRECT: Private setters
public string Name { get; private set; }

// ❌ WRONG: Throwing exceptions
if (name == null) throw new ArgumentNullException();

// ✅ CORRECT: Return Result
if (string.IsNullOrWhiteSpace(name))
    return Result.Failure<Entity>(EntityErrors.NameRequired);

// ❌ WRONG: Anemic domain model
public class User { public string Name { get; set; } }

// ✅ CORRECT: Rich domain model with behavior
public class User
{
    public string Name { get; private set; }
    public Result ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(UserErrors.NameRequired);
        Name = newName;
        RaiseDomainEvent(new UserNameChangedDomainEvent(Id, newName));
        return Result.Success();
    }
}
```
