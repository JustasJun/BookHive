using BookHive.Extensions;
using BookHive.Models;

namespace BookHive.Tests.Extensions;

public class SubjectExtensionsTests
{
    [Fact]
    public void ToGenre_WhenSubjectsAreNull_ReturnsUnknown()
    {
        IEnumerable<string>? subjects = null;

        Assert.Equal(Genre.Unknown, subjects.ToGenre());
    }

    [Fact]
    public void ToGenre_WhenSubjectsAreEmpty_ReturnsUnknown()
    {
        Assert.Equal(Genre.Unknown, Array.Empty<string>().ToGenre());
    }

    [Theory]
    [InlineData("Fantasy fiction", Genre.Fantasy)]
    [InlineData("SCIENCE FICTION", Genre.ScienceFiction)]
    [InlineData("Dystopian future", Genre.ScienceFiction)]
    [InlineData("Love stories", Genre.Romance)]
    [InlineData("Detective and mystery stories", Genre.Mystery)]
    [InlineData("Vampires", Genre.Horror)]
    [InlineData("Autobiography", Genre.Biography)]
    [InlineData("World history", Genre.Historical)]
    public void ToGenre_MatchesKeywordsIgnoringCase(string subject, Genre expected)
    {
        Assert.Equal(expected, new[] { subject }.ToGenre());
    }

    [Fact]
    public void ToGenre_WhenNothingMatches_ReturnsOther()
    {
        Assert.Equal(Genre.Other, new[] { "Cooking", "Gardening" }.ToGenre());
    }

    [Fact]
    public void ToGenre_WhenOneOfSeveralSubjectsMatches_ReturnsThatGenre()
    {
        Assert.Equal(Genre.Romance, new[] { "Cooking", "Romance" }.ToGenre());
    }

    [Fact]
    public void ToGenre_WhenSeveralGenresMatch_PrefersTheEarlierKeyword()
    {
        Assert.Equal(Genre.Fantasy, new[] { "History of fantasy" }.ToGenre());
    }
}