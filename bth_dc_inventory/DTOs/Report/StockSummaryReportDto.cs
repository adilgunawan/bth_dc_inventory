namespace bth_dc_inventory.DTOs.Report
{
    public class StockSummaryReportDto
    {
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalAssetValue { get; set; }
    }
}