
using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Users
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}

