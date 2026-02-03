namespace bth_dc_inventory.DTOs.Item
{
    public class ItemFilterDto
    {
        // 🔍 Global search (ItemCode, ItemName, AssetNumber, SerialNumber, PO)
        public string? Search { get; set; }

        // 🎯 Specific filters
        public int? CategoryId { get; set; }
        public int? DataCenterId { get; set; }


        //public string? PONumber { get; set; }
        public string? AssetNumber { get; set; }
        public string? SerialNumber { get; set; }

        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // 📄 Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

//using System;
//namespace bth_dc_inventory.DTOs.Item
//{
//    public class ItemFilterDto
//    {
//        public string? Search { get; set; }          
//        public int? CategoryId { get; set; }
//        public int? DataCenterId { get; set; }

//        public int Page { get; set; } = 1;
//        public int PageSize { get; set; } = 10;
//    }
//}