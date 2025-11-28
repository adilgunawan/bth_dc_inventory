using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bth_dc_inventory.Models
{
    public class Report
    {
        public int ID { get; set; } // Primary Key

        [Required]
        public int Month { get; set; } // Bulan laporan (misalnya 1 = Januari)

        [Required]
        public int Year { get; set; } // Tahun laporan

        [Required]
        public int CategoryId { get; set; } // Foreign Key ke tabel Category

        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = new Category(); // Navigasi ke Category

        [Required]
        [MaxLength(200)]
        public string ReportName { get; set; } = string.Empty; // Nama laporan

        [Required]
        [MaxLength(200)]
        public string FileExcel { get; set; } = string.Empty; // Nama file Excel laporan

        [Required]
        [MaxLength(200)]
        public string FilePdf { get; set; } = string.Empty; // Nama file PDF laporan

        [Required]
        public int TotalItems { get; set; } // Jumlah total item dalam laporan

        [Required]
        public DateTime GeneratedAt { get; set; } = DateTime.Now; // Tanggal laporan dibuat

        // Foreign Key ke tabel User
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = new User(); // Navigasi ke User
    }
}