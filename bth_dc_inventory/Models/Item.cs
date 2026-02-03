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

        //// ✅ NEW: PO Number (required)
        [Required]
        [MaxLength(200)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        // ✅ NEW: Asset Number (nullable)
        [MaxLength(100)]
        public string? AssetNumber { get; set; }

        // ✅ NEW: Serial Number (nullable)
        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        //// ✅ NEW: PO Number (required)
        //[Required]
        //[MaxLength(100)]
        //public string PONumber { get; set; } = string.Empty;

        // Category Relation
        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

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
//    [Index(nameof(ItemCode), IsUnique = true)]
//    public class Item
//    {
//        [Key]
//        public int Id { get; set; }

//        [Required]
//        [MaxLength(200)]
//        public string ItemCode { get; set; } = string.Empty;

//        [Required]
//        [MaxLength(200)]
//        public string ItemName { get; set; } = string.Empty;

//        // Category Relation
//        [Required]
//        public int CategoryId { get; set; }
//        public Category? Category { get; set; } // no instantiation!

//        // Data Center Relation
//        [Required]
//        public int DataCenterId { get; set; }
//        public DataCenter? DataCenter { get; set; }

//        // Buying price
//        [Required]
//        [Column(TypeName = "decimal(18,2)")]
//        public decimal BuyingPrice { get; set; }

//        // Quantity
//        [Required]
//        [Range(1, int.MaxValue)]
//        public int Quantity { get; set; }

//        [Required]
//        [MaxLength(20)]
//        public string Status { get; set; } = "active";

//        // Nullable purchase date
//        public DateTime? DateOfPurchase { get; set; }

//        // Created By (User relation)
//        [Required]
//        public int CreatedById { get; set; }
//        public User? CreatedBy { get; set; }

//        [Required]
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//        public DateTime? UpdatedAt { get; set; }
//    }
//}

