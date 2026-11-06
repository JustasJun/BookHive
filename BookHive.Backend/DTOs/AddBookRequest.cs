using BookHive.Models;

namespace BookHive.Backend.DTOs;

public record AddBookRequest(
    Book Book,
    ReadingStatus Status);
