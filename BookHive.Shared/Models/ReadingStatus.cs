using System.Text.Json.Serialization;

namespace BookHive.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReadingStatus
{
    WantToRead,
    CurrentlyReading,
    Read
}
