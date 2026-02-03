using System;
using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Item
{
    public class ItemUpdateDto
    {
        [Required]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        public string ItemName { get; set; } = string.Empty;

        public string? AssetNumber { get; set; }
        public string? SerialNumber { get; set; }

        //[Required]
        //public string PONumber { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int DataCenterId { get; set; }

        [Required]
        public decimal BuyingPrice { get; set; }

        public int Quantity { get; set; }
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
//        [StringLength(100, MinimumLength = 2)]
//        public string ItemCode { get; set; } = string.Empty;

//        [Required]
//        [StringLength(200, MinimumLength = 3)]
//        public string ItemName { get; set; } = string.Empty;

//        [Required]
//        public int CategoryId { get; set; }

//        [Required]
//        public int DataCenterId { get; set; }

//        [Required]
//        [Range(0, double.MaxValue)]
//        public decimal BuyingPrice { get; set; }

//        [Required]
//        [Range(0, int.MaxValue)]
//        public int Quantity { get; set; }

//        [Required]
//        [StringLength(20)]
//        public string Status { get; set; } = "active";

//        public DateTime? DateOfPurchase { get; set; }
//    }
//}

