using System;
namespace bth_dc_inventory.DTOs.Item
{
    public class ItemFilterDto
    {
        public string? Search { get; set; }          // ItemName / ItemCode
        public int? CategoryId { get; set; }
        public int? DataCenterId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}