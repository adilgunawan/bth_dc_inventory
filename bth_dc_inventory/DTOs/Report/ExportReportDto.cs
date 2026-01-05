namespace bth_dc_inventory.DTOs.Report
{
    public class ExportReportDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Format { get; set; } = "pdf";
    }
}