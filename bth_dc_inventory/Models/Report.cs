using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bth_dc_inventory.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        // Category (optional)
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        [MaxLength(200)]
        public string ReportName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? FileExcel { get; set; }

        [MaxLength(200)]
        public string? FilePdf { get; set; }

        [Required]
        public int TotalItems { get; set; }

        [Required]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // User
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}


//using System;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using Microsoft.EntityFrameworkCore;

//namespace bth_dc_inventory.Models
//{
//    public class Report
//    {
//        public int Id { get; set; } // Primary Key

//        [Required]
//        public int Month { get; set; } // Bulan laporan (misalnya 1 = Januari)

//        [Required]
//        public int Year { get; set; } // Tahun laporan

//        [Required]
//        public int CategoryId { get; set; } // Foreign Key ke tabel Category

//        [ForeignKey("CategoryId")]
//        public Category Category { get; set; } = new Category(); // Navigasi ke Category

//        [Required]
//        [MaxLength(200)]
//        public string ReportName { get; set; } = string.Empty; // Nama laporan

//        [Required]
//        [MaxLength(200)]
//        public string FileExcel { get; set; } = string.Empty; // Nama file Excel laporan

//        [Required]
//        [MaxLength(200)]
//        public string FilePdf { get; set; } = string.Empty; // Nama file PDF laporan

//        [Required]
//        public int TotalItems { get; set; } // Jumlah total item dalam laporan

//        [Required]
//        public DateTime GeneratedAt { get; set; } = DateTime.Now; // Tanggal laporan dibuat

//        // Foreign Key ke tabel User
//        [Required]
//        public int UserId { get; set; }

//        [ForeignKey("UserId")]
//        public User User { get; set; } = new User(); // Navigasi ke User
//    }
//}