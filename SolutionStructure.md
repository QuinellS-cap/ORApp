# ORApp Solution Structure Documentation

## Solution Overview

The ORApp solution is a comprehensive full-stack application built with .NET technologies, designed to support a sports prediction and betting platform. The application integrates data ingestion from external APIs (e.g., RapidAPI), user management, subscription services, payment processing, and real-time sports data management. It features a multi-platform frontend (MAUI for desktop/mobile), a Blazor web application, a RESTful API backend, a background worker for data processing, and a centralized data layer using Entity Framework Core with SQL Server.

The solution appears to be in early development stages, focusing on establishing the core infrastructure for a sports analytics and prediction service. Key business domains include user authentication, subscription management, payment integration, sports data aggregation (fixtures, teams, players, odds), and prediction analytics.

## Project Structure & Purposes

The solution consists of the following projects, each serving distinct architectural purposes:

- **ORApp** (MAUI Application): A cross-platform mobile and desktop application targeting Windows, macOS, and potentially other platforms. Contains platform-specific implementations and serves as the primary client for end-users on non-web platforms.

- **ORApp.Shared**: A shared library containing Blazor components, layouts, and common UI elements reused across web and potentially MAUI applications. Includes pages like Counter, Home, and Weather, suggesting early prototyping.

- **ORApp.Web**: A Blazor Server/WebAssembly application providing the web-based user interface. Includes base models and services for UI logic, following a component-based architecture.

- **ORApp.API**: An ASP.NET Core Web API project exposing RESTful endpoints. Currently minimal, with basic controller setup and OpenAPI documentation. Serves as the backend API for client applications.

- **ORApp.Data**: A class library containing the Entity Framework Core data context and all domain models. Acts as the data access layer with auto-generated models from a SQL Server database.

- **ORApp.Worker**: A background worker service using .NET Hosted Services. Configured with HTTP client for external API integration (RapidAPI) and database access for data ingestion and processing tasks.

## Technologies Used

- **Framework**: .NET 9.0 (latest LTS version)
- **Frontend**: 
  - MAUI (Multi-platform App UI) for cross-platform native apps
  - Blazor Server/WebAssembly for web UI with Razor components
  - Bootstrap CSS framework for styling
- **Backend**: ASP.NET Core Web API with OpenAPI/Swagger
- **Data Access**: Entity Framework Core 9.0 with SQL Server
- **Background Processing**: .NET Hosted Services with HttpClient for external API calls
- **Authentication/Authorization**: Not yet implemented (based on minimal current setup)
- **External Integrations**: RapidAPI for sports data ingestion
- **Development Tools**: EF Core Power Tools for code generation

## Architecture Analysis

The solution attempts to follow a layered architecture with separation of concerns:

- **Presentation Layer**: Split between MAUI (ORApp) and Blazor (ORApp.Web + ORApp.Shared)
- **API Layer**: ORApp.API providing REST endpoints
- **Business Logic**: Currently minimal, with most logic residing in data access
- **Data Layer**: ORApp.Data with EF Core context and models
- **Infrastructure**: ORApp.Worker handling background tasks and external integrations

The architecture shows early-stage development patterns:
- Dependency injection is used for DbContext and HttpClient registration
- Configuration-based connection strings and API keys
- Auto-generated EF models with Fluent API configuration in OnModelCreating
- Basic service registration in Program.cs files

However, the architecture deviates significantly from clean architecture principles, lacking proper abstraction layers and domain-driven design patterns.

## Deviations from Code-Rules.md

The current implementation shows several deviations from the established Code-Rules.md guidelines:

### Architecture Deviations (A-1, A-2, A-3)
- **Missing Required Projects**: The solution lacks ORApp.Application, ORApp.Domain, and ORApp.Infrastructure projects as mandated by A-1. Business logic is scattered across data and worker projects.
- **No Controller-Service-Repository Pattern**: ORApp.API has no controllers beyond the default WeatherForecast, and no service/repository abstractions are implemented (violates A-2).
- **MVVM Not Implemented**: ORApp.Web and ORApp.Shared do not follow MVVM pattern; components mix UI logic with data binding (violates A-3).

### Coding Practice Deviations (C-4, C-5, C-6, C-8)
- **Primitive Type IDs**: Models use raw Guid and int types instead of strongly-typed IDs (e.g., `public Guid UserId` instead of `UserId` record struct) (violates C-4).
- **Auto-generated Code**: EF Power Tools generates models without proper encapsulation or domain modeling (conflicts with C-6 preferring records for DTOs).
- **Synchronous Operations**: No evidence of async/await patterns in current code; database and HTTP operations may not be properly asynchronous (potential C-8 violation).

### Database Deviations (D-1, D-2)
- **Context Injection**: While DbContext is injected, there's no evidence of transaction management in service methods (D-1).
- **Schema Configuration**: Configuration is auto-generated and centralized, but lacks domain-specific business rules (partially compliant with D-2).

### Testing and Git Deviations (T-1, GH-1)
- **No Test Projects**: No ORApp.Tests project exists (violates T-1 SHOULD).
- **Commit History**: No visible commit history in codebase; cannot verify Conventional Commits compliance (GH-1).

## Data Model Highlights

The data model is comprehensive and sports-domain focused, containing 30+ entities:

### Core Domain Entities
- **Users**: Authentication and profile management with email uniqueness, soft deletes, and audit fields
- **Subscriptions & Payments**: Multi-tier subscription system with payment providers, statuses, and transaction tracking
- **Sports Data**: Complete sports ecosystem including:
  - Countries, Leagues, Seasons, Teams, Venues
  - Players, Coaches, PlayerPositions, PlayerTransfers
  - Fixtures, FixtureStatuses, FixtureEvents, Lineups
  - Odds, BettingMarkets, BettingOutcomes
  - Predictions, Statisticals, Standings

### Key Relationships
- Hierarchical sports structure: Country → League → Season → Fixture → Teams
- User-centric features: Sessions, PasswordResets, Payments, Subscriptions
- Betting infrastructure: Markets → Outcomes → Odds with provider tracking
- Event tracking: Fixtures → Events → Players/Teams with statistical data

### Notable Patterns
- **Audit Fields**: Most entities include DateCreated, DateUpdated, CreatedBy, UpdatedBy
- **Soft Deletes**: Users and other entities support IsDeleted flags
- **Status Management**: Separate status tables (PaymentStatuses, SubscriptionStatuses, FixtureStatuses)
- **External Integration**: ApiIngest and ProviderWebhookEvents for data synchronization
- **Active Flags**: IsActive on entities like Teams, Leagues, Venues for soft activation

## Current State & Notable Patterns

### Development Stage
The solution appears to be in MVP/prototype phase:
- Basic project structure established
- Database schema fully designed and implemented
- Minimal API and UI functionality
- Worker service configured for data ingestion
- No authentication/authorization implemented
- No business logic services developed

### Code Patterns Observed
- **Auto-generated Models**: Heavy reliance on EF Power Tools for model generation
- **Minimal Abstraction**: Direct DbContext usage without repository patterns
- **Configuration-based Setup**: Proper use of appsettings.json for connection strings and API keys
- **Shared Components**: Reusable Blazor components in ORApp.Shared
- **Platform-specific Code**: MAUI project with platform folders for Windows/macOS/Tizen

### Infrastructure Patterns
- **External API Integration**: HttpClient configured with headers for RapidAPI
- **Database-first Approach**: Schema exists with auto-generated context
- **Background Processing**: Hosted service pattern for data ingestion
- **Cross-platform UI**: Shared Blazor components with MAUI adaptation

## Recommendations

### Immediate Priorities
1. **Implement Clean Architecture**: Add missing ORApp.Application, ORApp.Domain, and ORApp.Infrastructure projects
2. **Introduce Strongly-typed IDs**: Replace primitive types with record structs (e.g., `UserId`, `FixtureId`)
3. **Add Authentication**: Implement JWT/session-based auth with ASP.NET Core Identity
4. **Develop Business Services**: Create service layer with proper dependency injection
5. **Implement Repository Pattern**: Abstract data access behind interfaces

### Architecture Improvements
1. **MVVM Implementation**: Restructure Blazor components to follow Model-View-ViewModel pattern
2. **API Development**: Build comprehensive REST endpoints with proper validation
3. **Error Handling**: Add global exception handling and logging
4. **Async Operations**: Ensure all I/O operations use async/await patterns

### Testing & Quality
1. **Add Test Project**: Create ORApp.Tests with unit and integration tests
2. **Code Coverage**: Implement automated testing for critical business logic
3. **FluentAssertions**: Use structured assertion libraries for better test readability

### Development Practices
1. **Conventional Commits**: Adopt semantic versioning commit format
2. **Code Reviews**: Implement peer review process for all changes
3. **Documentation**: Maintain API documentation and architecture decision records

### Security & Performance
1. **Input Validation**: Add comprehensive validation on all API endpoints
2. **Rate Limiting**: Implement API rate limiting for external integrations
3. **Caching**: Add caching layers for frequently accessed sports data
4. **Database Optimization**: Implement proper indexing and query optimization

This documentation provides a foundation for understanding the current state and guiding future development towards a more robust, maintainable architecture aligned with established best practices.