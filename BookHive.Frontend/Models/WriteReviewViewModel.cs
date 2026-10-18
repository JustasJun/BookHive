using System.ComponentModel.DataAnnotations;

namespace BookHive.Frontend.Models;

public class WriteReviewViewModel
{
    public string BookId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string? CoverId { get; set; }

    [Range(1, 5, ErrorMessage = "Choose a rating from 1 to 5")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Write something about the book")]
    [StringLength(2000)]
    public string Text { get; set; } = "";
}
