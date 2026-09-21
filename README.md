# BookHive

A social network for book lovers. Users can track their reading, rate and review books, build shelves, follow other readers and get recommendations.

Student project (ASP.NET MVC, .NET 10). Team: Ringailė Kaupaitė, Justas Junevičius, Rugilė Kazakevičiūtė, Meda Micaitė (lead).

## Run

Requires the .NET 10 SDK.

    dotnet run

The app starts at the address printed in the terminal.

## Structure

- `Controllers/`, `Views/`: MVC pages
- `Services/`: business logic and the Open Library client
- `Models/`: data types (`Book`, `Genre`, `LibraryBook`, `ReadingGoal`, ...)
- `Extensions/`: extension methods

## Status

Done:
- Book model and Open Library client (loads data via a stream)
- Genre detection from book subjects
- Personal library with reading statuses (in-memory for now)
- Reading goals with progress and pace calculation
- Base layout and styles

Planned:
- Book search page, user accounts, reviews and ratings, following, recommendations, book clubs, statistics

## Workflow

All changes go through pull requests with a short description and at least one review.