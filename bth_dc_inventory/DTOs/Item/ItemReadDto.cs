namespace bth_dc_inventory.DTOs.Item
{
    public class ItemReadDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;  //PO numebr
        public string ItemName { get; set; } = string.Empty;

        public string? AssetNumber { get; set; }
        public string? SerialNumber { get; set; }
        //public string PONumber { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;
        public string DataCenterName { get; set; } = string.Empty;
        public string? CategoryImage { get; set; }
        public decimal BuyingPrice { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime? DateOfPurchase { get; set; }
        public DateTime? UpdatedAt { get; set; }


    }
}


