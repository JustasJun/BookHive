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
