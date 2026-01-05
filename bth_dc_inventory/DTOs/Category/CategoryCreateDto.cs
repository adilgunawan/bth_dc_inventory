using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Category
{
    public class CategoryCreateDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(200, MinimumLength = 3,
            ErrorMessage = "Category name must be between 3 and 200 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;
    }
}