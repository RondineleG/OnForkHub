# OnForkHub Architecture Documentation

## Overview

OnForkHub follows a layered architecture. Dependencies point inward: presentation depends on application and infrastructure, which depend on the core domain.

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `OnForkHub.Core` | Entities, value objects, validation rules and the contracts (`Interfaces/Services`, `Interfaces/Repositories`, `Interfaces/UseCases`) |
| Application | `OnForkHub.Application` | Use cases, application services and request/response models (`Dtos`) |
| Infrastructure | `OnForkHub.Persistence` | Entity Framework Core (SQL Server) repositories and migrations |
| Shared | `OnForkHub.CrossCutting` | Authentication, caching, storage, middleware and dependency injection scanning |
| Presentation | `OnForkHub.Api`, `OnForkHub.Web` | Minimal API endpoints and the Blazor WebAssembly client |

## Naming

- Request and response models end with `Request` / `Response` (for example `CategoryRequest`, `CategorySearchResponse`, `PagedResponse<T>`). The `Dto` suffix is not used.
- Repository implementations are named after their storage, for example `CategoryRepositoryEF` implementing `ICategoryRepositoryEF`.

## Dependency Injection by Convention

Registration lives in `src/Presentations/OnForkHub.Api/Extensions/CommonServicesExtension.cs` and uses the scanner from `src/Shared/OnForkHub.CrossCutting/DependencyInjection`. New types that follow a convention need no manual registration:

| Convention | Scanned assembly | Registered as | Lifetime |
|---|---|---|---|
| Implements `IEndpointAsync` | `OnForkHub.Api` | `IEndpointAsync` | Scoped |
| Implements `IEntityValidator<>`, `IValidationRule<>` or `IValidationService<>` | `OnForkHub.Core` | Implemented interfaces | Scoped |
| Implements `IUseCase<,>` | `OnForkHub.Application` | Itself and implemented interfaces | Scoped |
| Implements an interface from `OnForkHub.Core.Interfaces.Services` or `OnForkHub.Core.Interfaces.Repositories` | `OnForkHub.Application`, `OnForkHub.Persistence` | Implemented interfaces | Scoped |

Only public, concrete, non-nested classes are picked up. To opt a class out, mark it with `[ExcludeFromRegistration]`. To control lifetime or service types for a single class, mark it with `[AutoRegister]`.

Registrations that do not fit a convention stay explicit in `AddExplicitServices`: open generics, singletons, hosted services and types without a contract. File storage is chosen by configuration (`FileStorage:Provider`).

The scanner exposes a fluent API for new conventions:

```csharp
services.Scan(scan => scan
    .FromAssemblyOf<SomeMarker>()
    .AddClassesImplementing(typeof(IHandler<>))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
```
