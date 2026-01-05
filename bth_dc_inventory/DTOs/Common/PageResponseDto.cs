using System;
namespace bth_dc_inventory.DTOs.Common
{
    public class PagedResponseDto<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public List<T> Data { get; set; } = new();
    }
}