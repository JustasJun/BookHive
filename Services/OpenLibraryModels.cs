using System.Text.Json.Serialization;

namespace BookHive.Services;

public class OpenLibrarySearchResponse  // API atsakymas
{
    [JsonPropertyName("docs")]
    public List<OpenLibraryDoc> Docs { get; set; } = new(); // get, set daro si lauka savybe (property)
}

public class OpenLibraryDoc
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("author_name")]
    public List<string>? AuthorNames { get; set; }

    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; set; }

    [JsonPropertyName("number_of_pages_median")]
    public int? PageCount { get; set; }

    [JsonPropertyName("cover_i")]
    public int? CoverId { get; set; }

    [JsonPropertyName("subject")] //temu sarasas, veliau is jo spesim zanra
    public List<string>? Subjects { get; set; }
}