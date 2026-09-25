namespace BookHive.Models;

public record Book(
    string Id,  // Id bus Open Library raktas (pvz. /works/OL45804W)
    string Title,
    string Author,
    int? FirstPublishYear,
    int? PageCount,
    string? CoverId,
    Genre Genre)
{
    public CoverImage Cover => new CoverImage(CoverId);

}