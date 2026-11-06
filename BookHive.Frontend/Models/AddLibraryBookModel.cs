using BookHive.Models;

namespace BookHive.Frontend.Models;

public record AddLibraryBookModel(
    Book Book,
    ReadingStatus Status);