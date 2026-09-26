using System.Text.Json.Serialization;

namespace BookHive.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Genre
{
    Unknown,
    Fantasy,
    ScienceFiction,
    Romance,
    Mystery,
    Horror,
    Historical,
    Biography,
    Other
}