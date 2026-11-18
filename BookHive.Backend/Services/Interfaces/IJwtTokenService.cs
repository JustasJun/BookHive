using BookHive.Models;

namespace BookHive.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GeneratePresenceTicket(int userId);
}
