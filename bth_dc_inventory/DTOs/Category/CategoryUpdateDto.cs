using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.Category
{
    public class CategoryUpdateDto
    {
        [Required(ErrorMessage = "Category ID is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(200, MinimumLength = 3,
            ErrorMessage = "Category name must be between 3 and 200 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}


//using System.ComponentModel.DataAnnotations;

//namespace bth_dc_inventory.DTOs.Category
//{
//    public class CategoryUpdateDto
//    {
//        [Required]
//        public int Id { get; set; }


//        [Required]
//        [StringLength(100, MinimumLength =3)]
//        public string Name { get; set; } = string.Empty;

//        [StringLength(200)]
//        public string Description { get; set; } = string.Empty ;




//    }
//}
