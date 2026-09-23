namespace BookHive.Models;

public record Book(
    string Id,  // Id bus Open Library raktas (pvz. /works/OL45804W)
    string Title,
    string Author,
    int? FirstPublishYear,
    int? PageCount,
    string? CoverId,
    Genre Genre) : IComparable<Book>
{
    public CoverImage Cover => new CoverImage(CoverId);

    public int CompareTo(Book? other) =>
         (FirstPublishYear ?? 0).CompareTo(other?.FirstPublishYear ?? 0);

}