namespace BookHive.Backend.DTOs;

public record RegisterRequest(
    string Username,
    string Email,
    string Password);