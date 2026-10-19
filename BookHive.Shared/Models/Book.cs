namespace BookHive.Models;

public record Book(
    string Id,
    string Title,
    string Author,
    int? FirstPublishYear,
    int? PageCount,
    string? CoverId,
    Genre Genre)
{
    public CoverImage Cover => new CoverImage(CoverId);
}
