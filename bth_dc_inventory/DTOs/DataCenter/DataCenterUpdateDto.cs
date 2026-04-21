using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.DTOs.DataCenter
{
    public class DataCenterUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Data Center name must be between 3 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200,
            ErrorMessage = "Location detail must not exceed 200 characters.")]
        public string LocationDetail { get; set; } = string.Empty;

        [Required]
        [StringLength(100,
            ErrorMessage = "Manager name must not exceed 100 characters.")]
        public string ManagerName { get; set; } = string.Empty;
    }
}
