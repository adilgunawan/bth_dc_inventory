using System;
using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Item
{
    public class ItemUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int DataCenterId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal BuyingPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "active";

        public DateTime? DateOfPurchase { get; set; }
    }
}

//using System;
//using System.ComponentModel.DataAnnotations;

//namespace bth_dc_inventory.DTOs.Item
//{
//    public class ItemUpdateDto
//    {
//        [Required]
//        public int Id { get; set; } // Id wajib untuk mencocokkan item yang akan diupdate

//        [Required]
//        [StringLength(100, MinimumLength = 2, ErrorMessage = "Item code must be between 2 and 100 characters.")]
//        public string ItemCode { get; set; } = string.Empty;

//        [Required]
//        [StringLength(200, MinimumLength = 3, ErrorMessage = "Item name must be between 3 and 200 characters.")]
//        public string ItemName { get; set; } = string.Empty;

//        [Required]
//        public int CategoryId { get; set; } // Foreign key to Category table

//        [Required]
//        public int DataCenterId { get; set; } // Foreign key to DataCenter table

//        [Required]
//        [Range(0, double.MaxValue, ErrorMessage = "Buying price must be a positive number.")]
//        public decimal BuyingPrice { get; set; }

//        [Required]
//        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or a positive number.")]
//        public int Quantity { get; set; }

//        [Required]
//        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
//        public string Status { get; set; } = string.Empty;

//        [Required]
//        public DateTime DateOfPurchase { get; set; }
//    }
//}