using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bth_dc_inventory.Models
{
    [Index(nameof(ItemCode), IsUnique = true)]
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        // Category Relation
        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; } // no instantiation!

        // Data Center Relation
        [Required]
        public int DataCenterId { get; set; }
        public DataCenter? DataCenter { get; set; }

        // Buying price
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyingPrice { get; set; }

        // Quantity
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "active";

        // Nullable purchase date
        public DateTime? DateOfPurchase { get; set; }

        // Created By (User relation)
        [Required]
        public int CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

//using System;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using Microsoft.EntityFrameworkCore;

//namespace bth_dc_inventory.Models
//{
//    public class Item
//    {
//        [Key]
//        public int Id { get; set; } // Primary Key

//        [Required]
//        [MaxLength(200)]
//        public string ItemCode { get; set; } = string.Empty; // Kode item tidak boleh null

//        [Required]
//        [MaxLength(200)]
//        public string ItemName { get; set; } = string.Empty; // Nama item tidak boleh null

//        [Required]
//        public int CategoryId { get; set; } // Foreign Key ke tabel Category

//        [ForeignKey("CategoryId")]
//        public Category Category { get; set; } = new Category(); // Navigasi ke Category

//        [Required]
//        public int DataCenterId { get; set; } // Foreign Key ke tabel DataCenter

//        [ForeignKey("DataCenterId")]
//        public DataCenter DataCenter { get; set; } = new DataCenter(); // Navigasi ke DataCenter

//        [Required]
//        [Column(TypeName = "decimal(18,2)")]
//        public decimal BuyingPrice { get; set; } // Harga pembelian

//        [Required]
//        public int Quantity { get; set; } // Jumlah

//        [Required]
//        [MaxLength(20)]
//        public string Status { get; set; } = "active"; // Status (default: active)

//        [Required]
//        public DateTime? DateOfPurchase { get; set; } // Tanggal pembelian di ubah jadi nuulable

//        public int CreatedById { get; set; } // Foreign Key ke tabel User (ID pengguna yang membuat data)

//        [ForeignKey("CreatedById")]
//        public User CreatedBy { get; set; } = new User(); // Navigasi ke User

//        [Required]
//        public DateTime CreatedAt { get; set; } = DateTime.Now; // Waktu pembuatan tidak boleh null

//        public DateTime? UpdatedAt { get; set; } // Waktu diperbarui dapat null
//    }
//}