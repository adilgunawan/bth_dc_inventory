namespace bth_dc_inventory.DTOs.Report
{
    public class CategoryReportDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }
}
