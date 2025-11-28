using System;
using System.ComponentModel.DataAnnotations;

namespace bth_dc_inventory.Models
{
    public class DataCenter
    {
        public int ID { get; set; } // Primary Key

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string LocationDetail { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ManagerName { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}