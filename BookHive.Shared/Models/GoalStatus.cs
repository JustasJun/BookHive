using System.Text.Json.Serialization;

namespace BookHive.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GoalStatus
{
    Active,
    Completed,
    Missed
}
