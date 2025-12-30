using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Category
{
    public class CategoryUpdateDto
    {
        [Required]
        public int Id { get; set; }
        
        
        [Required]
        [StringLength(100, MinimumLength =3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty ;



        
    }
}
