using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Item
{
    public class ItemCreateDto
    {
        [Required]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        public string ItemName { get; set; } = string.Empty;

        public string? AssetNumber { get; set; }

        public string? SerialNumber { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int DataCenterId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Buying price must be greater than zero.")]
        public decimal BuyingPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }
        public DateTime? DateOfPurchase { get; set; }

    }

}


