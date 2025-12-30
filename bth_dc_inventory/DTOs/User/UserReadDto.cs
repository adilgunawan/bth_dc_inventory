using System;
using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.User
{
    public class UserReadDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        // Properti opsional untuk format tanggal
        public string CreatedAtFormatted => CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
    }
}