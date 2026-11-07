namespace BookHive.Backend.DTOs;

public record FollowCountsResponse(
    int Followers,
    int Following);