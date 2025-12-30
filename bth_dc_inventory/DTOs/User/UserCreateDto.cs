using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.User
{

    public enum UserRole
    {
        User,
        Admin
    }

    public class UserCreateDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "user";
    }
}