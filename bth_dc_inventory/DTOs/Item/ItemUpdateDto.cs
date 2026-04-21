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



