using System;
using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.Models
{
    public class Category
    {
        public int ID { get; set; } // Primary Key

        [Required]
        [StringLength(200)]
        public string CategoryName { get; set; } = string.Empty; // Nama Kategori tidak boleh null

        [StringLength(500)]
        public string Description { get; set; } = string.Empty; // Deskripsi tidak boleh null

        [MaxLength(500)]
        public string Image { get; set; } //gambar per katergori

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Tanggal pembuatan tidak boleh null

        public DateTime? UpdatedAt { get; set; } // Tanggal diperbarui dapat null
    }
}