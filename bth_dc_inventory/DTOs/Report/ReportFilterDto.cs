namespace bth_dc_inventory.DTOs.Report
{
    public class ReportFilterDto
    {
        public int? CategoryId { get; set; }
        public int? DataCenterId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? Search { get; set; }
    }
}