using System.ComponentModel.DataAnnotations;

namespace BookHive.Backend.DTOs;

public sealed record UpdateProfileRequest
{
    [StringLength(30)]
    public string? DisplayName { get; init; }

    [StringLength(200)]
    public string? Bio { get; init; }

    [StringLength(2048)]
    public string? AvatarUrl { get; init; }
}
