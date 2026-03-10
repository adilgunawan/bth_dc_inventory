using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Users
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
