namespace bth_dc_inventory.DTOs.Report
{
    public class ItemReportDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;
        public string DataCenterName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal BuyingPrice { get; set; }

        public decimal TotalValue => Quantity * BuyingPrice;

        public DateTime DateOfPurchase { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}