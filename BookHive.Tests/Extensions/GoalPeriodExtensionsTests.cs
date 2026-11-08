using BookHive.Models;

namespace BookHive.Tests.Extensions;

public class GoalPeriodExtensionsTests
{
    private static readonly DateTime Start = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(GoalPeriod.OneWeek, 2026, 1, 22)]
    [InlineData(GoalPeriod.OneMonth, 2026, 2, 15)]
    [InlineData(GoalPeriod.ThreeMonths, 2026, 4, 15)]
    [InlineData(GoalPeriod.SixMonths, 2026, 7, 15)]
    [InlineData(GoalPeriod.OneYear, 2027, 1, 15)]
    public void AddTo_ReturnsEndOfThePeriod(
        GoalPeriod period, int year, int month, int day)
    {
        Assert.Equal(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc), period.AddTo(Start));
    }

    [Fact]
    public void AddTo_WhenPeriodIsUnknown_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((GoalPeriod)99).AddTo(Start));
    }

    [Theory]
    [InlineData(GoalPeriod.OneWeek, "1 week")]
    [InlineData(GoalPeriod.OneMonth, "1 month")]
    [InlineData(GoalPeriod.ThreeMonths, "3 months")]
    [InlineData(GoalPeriod.SixMonths, "6 months")]
    [InlineData(GoalPeriod.OneYear, "1 year")]
    public void Label_ReturnsReadableText(GoalPeriod period, string expected)
    {
        Assert.Equal(expected, period.Label());
    }

    [Fact]
    public void Label_WhenPeriodIsUnknown_FallsBackToEnumValue()
    {
        Assert.Equal("99", ((GoalPeriod)99).Label());
    }
}