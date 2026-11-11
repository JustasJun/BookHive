namespace BookHive.Services;

public static class PresenceAuth
{
    public const string PolicyName = "Presence";
    public const string PurposeClaim = "purpose";
    public const string PresencePurpose = "presence";

    public static readonly TimeSpan TicketLifetime = TimeSpan.FromMinutes(1);
}