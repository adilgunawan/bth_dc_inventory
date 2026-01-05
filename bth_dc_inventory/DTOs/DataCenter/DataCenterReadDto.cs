namespace bth_dc_inventory.DTOs.DataCenter
{
    public class DataCenterReadDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string LocationDetail { get; set; } = string.Empty;

        public string ManagerName { get; set; } = string.Empty;

        public int TotalItems { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
//namespace bth_dc_inventory.DTOs.DataCenter
//{
//    public class DataCenterReadDto
//    {
//        public int Id { get; set; }

//        public string Name { get; set; } = string.Empty;

//        public string LocationDetail { get; set; } = string.Empty;

//        public string ManagerName { get; set; } = string.Empty;

//        // Jumlah item dalam data center (hasil agregasi)
//        public int TotalItems { get; set; }

//        public DateTime CreatedAt { get; set; }

//        public DateTime? UpdatedAt { get; set; }
//    }
//}

////namespace bth_dc_inventory.DTOs.DataCenter
////{
////    public class DataCenterReadDto
////    {
////        public int Id { get; set; } 
////        public string Name { get; set; } = string.Empty;
////        public string Description { get; set; } = string.Empty;
////        public string Location {  get; set; } = string.Empty;
////        public string TotalItems {  get; set; } = string.Empty; 
////        public DateTime CreatedAt { get; set; }
////        public DateTime UpdatedAt { get; set; }
////    }
////}