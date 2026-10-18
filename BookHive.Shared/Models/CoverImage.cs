namespace BookHive.Models;

public readonly struct CoverImage
{
    public string? CoverId { get; }

    public CoverImage(string? coverId)
    {
        CoverId = coverId;
    }

    public string Url => CoverId == null
        ? "/images/no-cover.png"
        : $"https://covers.openlibrary.org/b/id/{CoverId}-M.jpg";
}
