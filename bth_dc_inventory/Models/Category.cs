using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace bth_dc_inventory.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Image { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation: Category → Items (1-to-Many)
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}

//using System;
//using System.ComponentModel.DataAnnotations;
//using System.Collections.Generic;
//using Microsoft.EntityFrameworkCore;

//namespace bth_dc_inventory.Models
//{
//    public class Category
//    {
//        public int Id { get; set; } // Primary Key

//        [Required]
//        [StringLength(200)]
//        public string CategoryName { get; set; } = string.Empty; // Nama Kategori tidak boleh null

//        [StringLength(500)]
//        public string Description { get; set; } = string.Empty; // Deskripsi tidak boleh null

//        [MaxLength(500)]
//        public string Image { get; set; } = string.Empty;


//        [Required]
//        public DateTime CreatedAt { get; set; } = DateTime.Now; // Tanggal pembuatan tidak boleh null

//        public DateTime? UpdatedAt { get; set; } // Tanggal diperbarui dapat null
//    }
//}