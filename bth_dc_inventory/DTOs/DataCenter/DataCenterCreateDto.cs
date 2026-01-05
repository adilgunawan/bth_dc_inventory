using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.DataCenter
{
    public class DataCenterCreateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Data center name must be between 3 and 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200, ErrorMessage = "Location detail must not exceed 200 characters.")]
        public string LocationDetail { get; set; } = string.Empty;

        [Required]
        [StringLength(200, ErrorMessage = "Manager name must not exceed 200 characters.")]
        public string ManagerName { get; set; } = string.Empty;
    }
}


//using System.ComponentModel.DataAnnotations;


//namespace bth_dc_inventory.DTOs.DataCenter
//{
//    public class DataCenterCreateDto
//    {
//       [Required]
//       [StringLength(100, MinimumLength =3, ErrorMessage = "Data Center name must be between 3 and 100 characters.")]
//       public string Name { get; set; } = string.Empty;

//        [StringLength(200, ErrorMessage = "Description Must not exceed 200 characters.")]
//        public string Description { get; set; } = string.Empty ;

//        [Required]
//        public string Location { get; set; } = string.Empty;




//    }
//}
