using System;

namespace bth_dc_inventory.DTOs.Item
{
    public class ItemReadDto
    {
        public int Id { get; set; }

        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string DataCenterName { get; set; } = string.Empty;

        public decimal BuyingPrice { get; set; }

        public int Quantity { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? DateOfPurchase { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Optional helper (frontend friendly)
        public string CreatedAtFormatted => CreatedAt.ToString("yyyy-MM-dd HH:mm");
    }
}


//using System;

//namespace bth_dc_inventory.DTOs.Item
//{
//    public class ItemReadDto
//    {
//        public int Id { get; set; }

//        public string ItemCode { get; set; } = string.Empty;

//        public string ItemName { get; set; } = string.Empty;

//        public string CategoryName { get; set; } = string.Empty; // Nama kategori dari relasi Category

//        public string DataCenterName { get; set; } = string.Empty; // Nama data center dari relasi DataCenter

//        public decimal BuyingPrice { get; set; }

//        public int Quantity { get; set; }

//        public string Status { get; set; } = string.Empty;

//        public DateTime DateOfPurchase { get; set; }

//        public DateTime? UpdatedAt { get; set; } = null;
//    }
//}