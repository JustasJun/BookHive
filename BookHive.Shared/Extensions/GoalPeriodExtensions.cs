namespace BookHive.Models;

public static class GoalPeriodExtensions
{
    public static DateTime AddTo(this GoalPeriod period, DateTime start)
        => period switch
        {
            GoalPeriod.OneWeek => start.AddDays(7),
            GoalPeriod.OneMonth => start.AddMonths(1),
            GoalPeriod.ThreeMonths => start.AddMonths(3),
            GoalPeriod.SixMonths => start.AddMonths(6),
            GoalPeriod.OneYear => start.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };

    public static string Label(this GoalPeriod period)
        => period switch
        {
            GoalPeriod.OneWeek => "1 week",
            GoalPeriod.OneMonth => "1 month",
            GoalPeriod.ThreeMonths => "3 months",
            GoalPeriod.SixMonths => "6 months",
            GoalPeriod.OneYear => "1 year",
            _ => period.ToString()
        };
}