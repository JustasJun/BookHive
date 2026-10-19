using System.Text.Json.Serialization;

namespace BookHive.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GoalPeriod
{
    OneWeek,     // 0
    OneMonth,    // 1
    ThreeMonths, // 2
    SixMonths,   // 3
    OneYear      // 4
}