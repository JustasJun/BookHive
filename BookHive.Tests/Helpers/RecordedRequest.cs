namespace BookHive.Tests.Helpers;

public sealed record RecordedRequest(
    string Method,
    string PathAndQuery,
    string Body,
    string? Authorization);
