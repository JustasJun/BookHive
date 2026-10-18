namespace BookHive.Backend.DTOs;

public record LoginRequest(
    string LoginInput,
    string Password);