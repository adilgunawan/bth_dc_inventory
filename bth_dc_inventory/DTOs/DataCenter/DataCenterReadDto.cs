namespace bth_dc_inventory.DTOs.DataCenter
{
    public class DataCenterReadDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location {  get; set; } = string.Empty;
        public string TotalItems {  get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}