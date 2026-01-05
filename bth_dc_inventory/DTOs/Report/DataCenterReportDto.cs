namespace bth_dc_inventory.DTOs.Report
{
    public class DataCenterReportDto
    {
        public int DataCenterId { get; set; }
        public string DataCenterName { get; set; } = string.Empty;

        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }
}