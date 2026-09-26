namespace BookHive.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly RegisteredAt { get; set; }

    // Sekimo funkcionalumui (follow) — kitų vartotojų Id, kuriuos šis vartotojas seka,
    // ir tų, kurie seka jį patį
    public List<int> FollowingIds { get; set; } = new();

    public List<int> FollowerIds { get; set; } = new();

    public User(int id, string username, string email, string passwordHash)
    {
        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        RegisteredAt = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}