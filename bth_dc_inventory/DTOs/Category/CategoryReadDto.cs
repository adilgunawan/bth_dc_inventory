namespace bth_dc_inventory.DTOs.Category
{
    public class CategoryReadDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;    
        public string Description { get; set; } = string.Empty; 
        public int TotalItems {  get; set; }
    }
}
