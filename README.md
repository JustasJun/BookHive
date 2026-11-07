# BookHive

A social network for book lovers. Users can track their reading, rate and review books, build shelves, follow other readers and get recommendations.

Student project (ASP.NET, .NET 10). Team:

* Ringailė Kaupaitė
* Justas Junevičius
* Rugilė Kazakevičiūtė
* Meda Micaitė (lead)

## Architecture

The solution has three projects. The UI talks to the API over REST.

```
BookHive.Frontend  (ASP.NET MVC)  --REST-->  BookHive.Backend  (ASP.NET Web API)  -->  PostgreSQL
                                                         |
                                                         +-->  Open Library API
              \                                          /
               +------- BookHive.Shared (models) --------+
```

* `BookHive.Backend`: the API. It is split into layers:
  * **Controllers** only receive the HTTP request, call a service and return the HTTP result.
  * **Services** contain the business logic (validation, rules, progress calculation, book filtering and sorting).
  * **Repositories** (`I...Repository` + implementation) are the only place that talks to the database through the EF Core context.
  * `OpenLibraryClient` loads book data from Open Library (the response is read as a stream).
  * Extension methods, EF Core context and migrations.
* `BookHive.Frontend`: MVC views, styles and scripts. It calls the backend API and contains no business logic.
* `BookHive.Shared`: models used by both projects (`Book`, `Review`, `ReadingGoal`, `GoalProgress`, `Genre`, ...).

## Run

Requires the .NET 10 SDK and a PostgreSQL database.

1. Set the database connection string (stored in user secrets, do not commit it):

```
dotnet user-secrets set "ConnectionStrings:BookHiveDatabase" "Host=...;Port=5432;Database=...;Username=...;Password=..." --project BookHive.Backend
```

2. Create or update the database tables:

```
dotnet ef database update --project BookHive.Backend
```

3. Start the backend first, then the frontend, each in its own terminal:

```
dotnet run --project BookHive.Backend    # http://localhost:5254
dotnet run --project BookHive.Frontend   # http://localhost:5176
```

Open http://localhost:5176. The frontend needs the backend running to load any data.

## Status

Done:

* Registration and login (passwords are hashed, the user is signed in with a cookie)
* Book catalog: search via Open Library, genre detection from subjects, filtering by several genres, sorting, cover images
* Personal library stored in the database: add a book from the search results, change its reading status, remove it
* Reviews: write a review with a 1-5 rating, see your own posts on the "My posts" page and delete them
* Reading goals: choose a goal and a time period, see progress and pace, and goal history (completed and missed goals)
* Database: EF Core + PostgreSQL with migrations

Planned:

* Custom shelves and reading progress (pages / percent)
* Following other readers, comments and likes, a real activity feed (the feed currently shows only your own reviews)
* Badges and achievements

## Workflow

All changes go through pull requests with a description of what was done and why, and at least one review from a teammate.

## JWT Authentication Setup

BookHive uses JWT authentication between the Frontend and Backend.

The Backend generates JWT access tokens after successful registration or login. The Frontend stores the token in its protected authentication cookie and automatically attaches it to authenticated Backend API requests.

### Local development setup

Each developer must configure a JWT signing key locally before starting the Backend.

**1. Generate a secure random key**

Run the following commands in PowerShell:

```powershell
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$bytes = New-Object byte[] 32
$rng.GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)
$rng.Dispose()
```

**2. Store the key using .NET User Secrets**

From the solution root, run:

```powershell
dotnet user-secrets set "Jwt:Key" "$key" --project BookHive.Backend
```

Each developer can generate their own key. The key does not need to be shared between developers.

**3. Run the application**

Start the Backend and Frontend as usual.

The Backend requires the JWT signing key to start successfully. If the key is missing, the application will throw a configuration error.

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

### Verification

After configuring the key, run:

```powershell
dotnet build
dotnet test
```

Then manually verify registration, login, reading goals, library operations, reviews, and logout.