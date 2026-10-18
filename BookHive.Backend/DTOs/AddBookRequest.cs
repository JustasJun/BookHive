using BookHive.Models;

namespace BookHive.Backend.DTOs;

public record AddBookRequest(
    int UserId,
    Book Book,
    ReadingStatus Status);
