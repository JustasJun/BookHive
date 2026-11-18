using System.Text.Json.Serialization;

namespace BookHive.Services;

public class OpenLibrarySearchResponse
{
    [JsonPropertyName("docs")]
    public List<OpenLibraryDoc> Docs { get; set; } = new();
}
