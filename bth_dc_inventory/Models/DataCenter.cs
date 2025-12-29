using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace bth_dc_inventory.Models
{
    public class DataCenter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string LocationDetail { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ManagerName { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation: One DataCenter has many Items
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}

//using System;
//using System.ComponentModel.DataAnnotations;
//using System.Collections.Generic;
//using Microsoft.EntityFrameworkCore;

//namespace bth_dc_inventory.Models
//{
//    public class DataCenter
//    {
//        public int Id { get; set; } // Primary Key

//        [Required]
//        [StringLength(200)]
//        public string Name { get; set; } = string.Empty;

//        [Required]
//        [StringLength(200)]
//        public string LocationDetail { get; set; } = string.Empty;

//        [Required]
//        [StringLength(200)]
//        public string ManagerName { get; set; } = string.Empty;

//        [Required]
//        public DateTime CreatedAt { get; set; } = DateTime.Now;

//        public DateTime? UpdatedAt { get; set; }
//    }
//}
