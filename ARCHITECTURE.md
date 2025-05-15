# OnForkHub Architecture Documentation

## Overview

This document provides an overview of the architectural decisions and improvements made to the OnForkHub application. The primary goal of these changes is to implement SOLID principles, reduce strong coupling between components, and optimize class responsibilities.

## Architectural Improvements

### 1. Dependency Injection and Interfaces

To reduce strong coupling and improve testability, interfaces were created for all major services and dependencies. The following interfaces were introduced:

- `ICategoryService`
- `ICategoryRepositoryEF`
- `IValidationService<Category>`

These interfaces were registered in the dependency injection container in `src/Core/OnForkHub.Application/Extensions/DependencyInjection.cs`.

### 2. Single Responsibility Principle

Services were refactored to ensure they have single, well-defined responsibilities. For example, the `CategoryService` in `src/Core/OnForkHub.Application/Services/CategoryService.cs` was refactored to handle only category-related operations. Validation logic was extracted into separate validation services, such as `CategoryValidationService` in `src/Core/OnForkHub.Core/Validations/Categories/CategoryValidationService.cs`.

### 3. Class Hierarchies

Class hierarchies were reviewed and optimized to ensure they are well-structured and follow SOLID principles. Base classes like `BaseService` in `src/Core/OnForkHub.Application/Services/Base/BaseService.cs` and `BaseEntity` in `src/Core/OnForkHub.Core/Entities/Base/BaseEntity.cs` were reviewed to ensure they do not contain unnecessary logic.

## Conclusion

These architectural improvements aim to enhance the maintainability, testability, and scalability of the OnForkHub application. By adhering to SOLID principles and reducing strong coupling, the application is better structured and easier to extend in the future.
