using System.ComponentModel.DataAnnotations;


namespace bth_dc_inventory.DTOs.DataCenter
{
    public class DataCenterUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Data Center name must be  between 3 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Location  { get; set; }   = string.Empty ;

    }
}
