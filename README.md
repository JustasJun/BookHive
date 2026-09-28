# BookHive

A social network for book lovers. Users can track their reading, rate and review books, build shelves, follow other readers and get recommendations.

Student project (ASP.NET, .NET 10). 
Team: 
- Ringailė Kaupaitė,
- Justas Junevičius,
- Rugilė Kazakevičiūtė,
- Meda Micaitė (lead).

## Architecture

The solution has three projects. The UI talks to the API over REST.

    BookHive.Frontend  (ASP.NET MVC)  --REST-->  BookHive.Backend  (ASP.NET Web API)  -->  Open Library API
                  \                                     /
                   +------ BookHive.Shared (models) ----+

- `BookHive.Backend`: controllers returning JSON, services (Open Library client, book filtering/sorting, library, reading goals), extension methods, EF Core context and migrations.
- `BookHive.Frontend`: MVC views, styles and scripts. It contains no business logic.
- `BookHive.Shared`: models used by both projects (`Book`, `Genre`, `LibraryBook`, `ReadingGoal`, ...).

## Run

Requires the .NET 10 SDK. Start the backend first, then the frontend, each in its own terminal:

    dotnet run --project BookHive.Backend    # http://localhost:5254
    dotnet run --project BookHive.Frontend   # http://localhost:5176

Open http://localhost:5176. The frontend needs the backend running to load any data.

## Status

Done:
- Book catalog: search via Open Library (data read as a stream), genre detection from subjects, filtering by several genres, four sort options, cover images
- Personal library: add a book from the search results (in-memory for now)
- Reading goals: set a yearly goal and see progress and pace
- Login and registration pages (UI only, no real authentication yet)
- Database schema: EF Core + PostgreSQL migration (not connected to the services yet)

Planned:
- Connect the library to the database and show book titles in it
- Real authentication, reviews and ratings, following, recommendations, book clubs, statistics

## Workflow

All changes go through pull requests with a description of what was done, and at least one review.
