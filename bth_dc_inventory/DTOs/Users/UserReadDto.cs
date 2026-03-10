using System;
using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Users
{
    public class UserReadDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // Optional helper (boleh dipakai di frontend)
        public string CreatedAtFormatted => CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

