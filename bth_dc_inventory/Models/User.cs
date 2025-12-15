using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace bth_dc_inventory.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        [StringLength(200)]
        public string Username { get; set; } = string.Empty; // Non-nullable, dengan default value

        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // Email wajib dan non-nullable

        [Required]
        public string Password { get; set; } = string.Empty; // Non-nullable, untuk password (bisa di-hash)

        [Required]
        [StringLength(10)]
        public string Role { get; set; } = "user"; // Role sebagai "user" atau "admin" (default "user")

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Non-nullable, default sekarang

        public DateTime? UpdatedAt { get; set; } // Bisa null untuk tanggal diperbaruiW

        // Tambahkan Properti Navigasi untuk Laporan
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<Item> CreatedItems { get; set; } = new List<Item>();
    }
}