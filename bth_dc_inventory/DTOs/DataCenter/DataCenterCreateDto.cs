using System.ComponentModel.DataAnnotations;


namespace bth_dc_inventory.DTOs.DataCenter
{
    public class DataCenterCreateDto
    {
       [Required]
       [StringLength(100, MinimumLength =3, ErrorMessage = "Data Center name must be between 3 and 100 characters.")]
       public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description Must not exceed 200 characters.")]
        public string Description { get; set; } = string.Empty ;

        [Required]
        public string Location { get; set; } = string.Empty;




    }
}
