using BookHive.Models;

namespace BookHive.Backend.DTOs;

public record ChangeStatusRequest(
    int UserId,
    string BookId,
    ReadingStatus Status);
