using System;
using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Item
{
    public class ItemCreateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 5,
            ErrorMessage = "Item Code (Serial Number) must be between 5 and 200 characters.")]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 3,
            ErrorMessage = "Item name must be between 3 and 200 characters.")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Data Center is required.")]
        public int DataCenterId { get; set; }

        [Required]
        [Range(0, double.MaxValue,
            ErrorMessage = "Buying price must be a positive number.")]
        public decimal BuyingPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfPurchase { get; set; }
    }
}


//using System;
//using System.ComponentModel.DataAnnotations;


//namespace bth_dc_inventory.DTOs.Item
//{
//    public class ItemCreateDto
//    {
//        [Required]
//        [StringLength(100, MinimumLength = 2, ErrorMessage = "Item Code must be between 2 and 100 characters.")]
//        public string ItemCode { get; set; } = string.Empty;


//        [Required]
//        [StringLength(200, MinimumLength =3, ErrorMessage ="Item name must be between 3 and 200 characters.")]
//        public string ItemName { get; set; } = string.Empty;

//        [Required]
//        public int CategoryId { get; set; }

//        [Required]
//        public int DataCenterId { get; set; } 


//        [Required]
//        [Range(0, double.MaxValue, ErrorMessage = "Buying price must be a positif number.")]
//        public decimal BuyingPrice { get; set; }


//    }
//}
