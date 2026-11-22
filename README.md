# BookHive

A social network for book lovers. Users can discover books, track their reading, write reviews, set reading goals, follow other readers, and interact with the reading community.

Student project (ASP.NET, .NET 10, PostgreSQL). Team:

* Ringailė Kaupaitė
* Justas Junevičius
* Rugilė Kazakevičiūtė
* Meda Micaitė (lead)

## Architecture

The solution contains four projects. The Frontend communicates with the Backend through REST API requests.

```text
BookHive.Frontend (ASP.NET Core MVC)
        |
        | REST API (HTTP + JWT)
        v
BookHive.Backend (ASP.NET Core Web API)
        |
        +----> PostgreSQL (Entity Framework Core)
        |
        +----> Open Library API

BookHive.Shared
    Common models shared by Frontend and Backend

BookHive.Tests
    Unit and integration tests
```

## Backend

BookHive.Backend follows a layered architecture:

- __Controllers__ handle HTTP requests, call services, and return HTTP responses.
- __Services__ implement business logic, validation, reading goal calculations, book filtering, and sorting.
- __Repositories__ handle database operations using Entity Framework Core. Repository interfaces separate data access from business logic.
- __Data__ contains BookHiveDbContext and database configuration.
- __Migrations__ contain Entity Framework Core database schema changes.
- __Exceptions__ contains the custom BookHiveException.
- __Extensions__ contains extension methods and dependency injection registrations.
- __Hubs__ contains SignalR functionality for online user presence.
  
OpenLibraryClient retrieves book information from the external Open Library API.

## Frontend

BookHive.Frontend is an ASP.NET Core MVC application containing controllers, views, view models, CSS, and JavaScript.

It communicates with the Backend through HTTP requests and handles user interaction and presentation.

## Shared

BookHive.Shared contains models and other shared types used by both Frontend and Backend, including books, reviews, reading goals, and community-related models.

## Tests

BookHive.Tests contains automated tests organized into:

- UnitTests/Controllers – controller behavior and HTTP results.
- UnitTests/Services – business logic.
- UnitTests/Repositories – database operations.
- UnitTests/Extensions – extension methods.
- Integration – HTTP request processing, authentication, authorization, and book search.
- Helpers – shared testing utilities, factories, and HTTP stubs.

## Technologies

- C# and .NET 10
- ASP.NET Core MVC and Web API
- Entity Framework Core
- PostgreSQL
- JWT authentication
- Cookie authentication
- SignalR
- Open Library API
- xUnit
- NSubstitute
- Coverlet (code coverage)

## Run

Requires the .NET 10 SDK, PostgreSQL and EF Core CLI tools (dotnet ef).

1. Set the database connection string (stored in user secrets, do not commit it):

```
dotnet user-secrets set "ConnectionStrings:BookHiveDatabase" "Host=...;Port=5432;Database=...;Username=...;Password=..." --project BookHive.Backend
```

2. Configure JWT authentication. Generate a secure random key using PowerShell:

```powershell
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$bytes = New-Object byte[] 32
$rng.GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)
$rng.Dispose()
```
Store the generated key using .NET User Secrets:

```powershell
dotnet user-secrets set "Jwt:Key" "$key" --project BookHive.Backend
```

3. Create or update the database tables:

```
dotnet ef database update --project BookHive.Backend
```

4. Start the backend first, then the frontend, each in its own terminal:

```
dotnet run --project BookHive.Backend    # http://localhost:5254
dotnet run --project BookHive.Frontend   # http://localhost:5176
```

Open http://localhost:5176. The frontend needs the backend running to load any data.

## Status

Done:

- Registration and login (password hashing, JWT authentication, cookies)
- User profiles: view and edit profile information, search for users
- Book catalog: search via Open Library, genre detection, filtering, sorting, cover images
- Personal library: add books, change reading status, remove books
- Reviews: write reviews with 1-5 ratings, view and delete personal reviews
- Community: follow/unfollow users, followers and following lists, community review feed
- Online user presence using SignalR
- Reading goals: set goals and time periods, track progress and pace, view goal history
- Database: EF Core + PostgreSQL with migrations
- Automated unit and integration tests

Planned:

- Custom shelves and detailed reading progress (pages / percent)
- Badges and achievements

## Workflow

All changes go through pull requests with a description of what was done and why, and at least one review from a teammate.

## Authentication and Security

### Authentication behavior

- JWT access tokens expire after 2 hours.
- The Frontend authentication cookie also expires after 2 hours.
- Protected Backend endpoints require a valid Bearer token.
- The Backend identifies the authenticated user using the validated JWT rather than trusting user IDs supplied in requests.
- Public endpoints, such as book searches, do not require authentication.

### Security notes

- Never commit JWT signing keys to Git.
- Do not store signing keys in `appsettings.json`.
- Do not expose access tokens to browser JavaScript.
- For deployment, configure `Jwt__Key` using an environment variable or a secure secret-management service.
- The JWT issuer and audience are configured in the Backend application settings.

## Verification

After configuring the key, run:

```powershell
dotnet build
dotnet test
```

Manually verify important user flows when relevant, including registration, login, profile editing, library operations, reviews, following other users, reading goals, and logout.
