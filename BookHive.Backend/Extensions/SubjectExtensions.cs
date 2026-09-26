using BookHive.Models;

namespace BookHive.Extensions;
public static class SubjectExtensions
{
    private static readonly (string Keyword, Genre Value) [] GenreKeywords =
    {
        ("fantasy", Genre.Fantasy),
        ("wizards", Genre.Fantasy),
        ("dragons", Genre.Fantasy),

        ("science fiction", Genre.ScienceFiction),
        ("space opera", Genre.ScienceFiction),
        ("time travel", Genre.ScienceFiction),
        ("dystopi", Genre.ScienceFiction),

        ("romance", Genre.Romance),
        ("love stories", Genre.Romance),

        ("mystery", Genre.Mystery),
        ("detective", Genre.Mystery),
        ("thriller", Genre.Mystery),
        ("suspense", Genre.Mystery),

        ("horror", Genre.Horror),
        ("ghost stories", Genre.Horror),
        ("vampires", Genre.Horror),

        ("biograph", Genre.Biography),
        ("memoir", Genre.Biography),
    
        ("histor", Genre.Historical)
    };

    public static Genre ToGenre (this IEnumerable<string>? subjects)
    {
        if (subjects == null || !subjects.Any())
        {
            return Genre.Unknown;
        }

        foreach (var (keyword, genre) in GenreKeywords)
        {
            bool found = subjects.Any(s => s.Contains(keyword, StringComparison.OrdinalIgnoreCase)); // LINQ

            if (found)
            {
                return genre;
            }

        }

        return Genre.Other;
    }
}