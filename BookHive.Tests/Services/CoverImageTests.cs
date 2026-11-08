using BookHive.Models;

namespace BookHive.Tests.Services;

public class CoverImageTests
{
    [Fact]
    public void Url_WhenThereIsNoCoverId_ReturnsPlaceholder()
    {
        var cover = new CoverImage(null);

        Assert.Equal("/images/no-cover.png", cover.Url);
    }

    [Fact]
    public void Url_WhenThereIsACoverId_PointsToOpenLibraryCovers()
    {
        var cover = new CoverImage("123");

        Assert.Equal("https://covers.openlibrary.org/b/id/123-M.jpg", cover.Url);
    }
}