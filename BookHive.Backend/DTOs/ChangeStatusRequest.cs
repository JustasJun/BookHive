using BookHive.Models;

namespace BookHive.Backend.DTOs;

public record ChangeStatusRequest(
    string BookId,
    ReadingStatus Status);
