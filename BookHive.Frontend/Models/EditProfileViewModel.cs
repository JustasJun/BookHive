using System.ComponentModel.DataAnnotations;

namespace BookHive.Frontend.Models;

public class EditProfileViewModel
{
    [StringLength(30, ErrorMessage = "Display name must be 30 characters or fewer.")]
    [Display(Name = "Display name")]
    public string? DisplayName { get; set; }

    [StringLength(200, ErrorMessage = "Bio must be 200 characters or fewer.")]
    [Display(Name = "Bio")]
    public string? Bio { get; set; }

    [StringLength(2048, ErrorMessage = "Avatar URL must be 2048 characters or fewer.")]
    [Display(Name = "Avatar URL")]
    public string? AvatarUrl { get; set; }
}
