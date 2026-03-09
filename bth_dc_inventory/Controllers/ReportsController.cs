using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.Models;
using QuestPDF.Fluent;
using OfficeOpenXml;
using QuestPDF.Helpers;

namespace bth_dc_inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================
        // GET: api/reports/stats
        // =====================================
        [HttpGet("stats")]
        public async Task<IActionResult> GetReportStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                // Default to last 30 days if no dates provided
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1);

                // Apply consistent date filtering
                var query = _context.Items.AsQueryable();

                if (startDate.HasValue || endDate.HasValue)
                {
                    query = query.Where(i => i.CreatedAt >= start && i.CreatedAt < end);
                }

                var totalEntries = await query.CountAsync();
                var newItems = await query.CountAsync(i => i.CreatedAt >= start);
                var updatedItems = await query.CountAsync(i => i.UpdatedAt.HasValue && i.UpdatedAt >= start);
                var totalDataCenters = await _context.DataCenters.CountAsync();

                // Category breakdown - Fixed null reference
                var categoryStats = await query
                    .Include(i => i.Category)
                    .Where(i => i.Category != null) // Add null check
                    .GroupBy(i => i.Category.CategoryName)
                    .Select(g => new
                    {
                        Category = g.Key ?? "Unknown",
                        Count = g.Count(),
                        Percentage = totalEntries > 0 ? Math.Round((double)g.Count() / totalEntries * 100, 1) : 0
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                // Data center breakdown - Fixed null reference
                var dataCenterStats = await query
                    .Include(i => i.DataCenter)
                    .Where(i => i.DataCenter != null) // Add null check
                    .GroupBy(i => i.DataCenter.Name)
                    .Select(g => new
                    {
                        DataCenter = g.Key ?? "Unknown",
                        Count = g.Count(),
                        Percentage = totalEntries > 0 ? Math.Round((double)g.Count() / totalEntries * 100, 1) : 0
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                // Status breakdown - Fixed null reference
                var statusStats = await query
                    .GroupBy(i => i.Status ?? "Unknown")
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalEntries = totalEntries,
                    NewItems = newItems,
                    UpdatedItems = updatedItems,
                    TotalDataCenters = totalDataCenters,
                    CategoryStats = categoryStats,
                    DataCenterStats = dataCenterStats,
                    StatusStats = statusStats,
                    DateRange = new { Start = start, End = end }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching report stats", error = ex.Message });
            }
        }

        // =====================================
        // GET: api/reports/data
        // =====================================
        [HttpGet("data")]
        public async Task<IActionResult> GetReportData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.Items
                    .Include(i => i.Category)
                    .Include(i => i.DataCenter)
                    .Include(i => i.CreatedBy)
                    .AsQueryable();

                // Apply date filter
                if (startDate.HasValue && endDate.HasValue)
                {
                    var start = startDate.Value.Date;
                    var end = endDate.Value.Date.AddDays(1);
                    query = query.Where(i => i.CreatedAt >= start && i.CreatedAt < end);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new
                    {
                        Id = i.Id,
                        ItemCode = i.ItemCode ?? "N/A",
                        ItemName = i.ItemName ?? "N/A",
                        CategoryName = i.Category != null ? i.Category.CategoryName : "Unknown",
                        DataCenterName = i.DataCenter != null ? i.DataCenter.Name : "Unknown",
                        Quantity = i.Quantity,
                        Status = i.Status ?? "Unknown",
                        CreatedAt = i.CreatedAt,
                        UpdatedAt = i.UpdatedAt,
                        CreatedByName = i.CreatedBy != null ? i.CreatedBy.Username : "System",
                        BuyingPrice = i.BuyingPrice,
                        AssetNumber = i.AssetNumber,
                        SerialNumber = i.SerialNumber
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Data = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching report data", error = ex.Message });
            }
        }

        // =====================================
        // GET: api/reports/export/pdf
        // =====================================
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPDF([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1);

                Console.WriteLine($"PDF Export - Date Range: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");

                var items = await _context.Items
                    .Include(i => i.Category)
                    .Include(i => i.DataCenter)
                    .Include(i => i.CreatedBy)
                    .Where(i => i.CreatedAt >= start && i.CreatedAt < end)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                Console.WriteLine($"Found {items.Count} items for PDF export");

                if (!items.Any())
                {
                    return NotFound(new { message = "No data found for the specified date range" });
                }

                var pdfBytes = GenerateReportPdf(items, start, end);

                Console.WriteLine($"Generated PDF size: {pdfBytes.Length} bytes");

                return File(
                    pdfBytes,
                    "application/pdf",
                    $"InventoryReport_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Export Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message, details = ex.StackTrace });
            }
        }

        // =====================================
        // GET: api/reports/export/excel
        // =====================================
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1);

                Console.WriteLine($"Excel Export - Date Range: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");

                var items = await _context.Items
                    .Include(i => i.Category)
                    .Include(i => i.DataCenter)
                    .Include(i => i.CreatedBy)
                    .Where(i => i.CreatedAt >= start && i.CreatedAt < end)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                Console.WriteLine($"Found {items.Count} items for Excel export");

                if (!items.Any())
                {
                    return NotFound(new { message = "No data found for the specified date range" });
                }

                var excelBytes = GenerateReportExcel(items, start, end);

                Console.WriteLine($"Generated Excel size: {excelBytes.Length} bytes");

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"InventoryReport_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excel Export Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error generating Excel", error = ex.Message, details = ex.StackTrace });
            }
        }

        // =====================================
        // TEST ENDPOINT
        // =====================================
        [HttpGet("test")]
        public async Task<IActionResult> TestData([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1);

                var totalItems = await _context.Items.CountAsync();
                var itemsInRange = await _context.Items
                    .Where(i => i.CreatedAt >= start && i.CreatedAt < end)
                    .CountAsync();

                var sampleItems = await _context.Items
                    .Include(i => i.Category)
                    .Include(i => i.DataCenter)
                    .Take(5)
                    .Select(i => new
                    {
                        i.Id,
                        i.ItemName,
                        i.CreatedAt,
                        CategoryName = i.Category != null ? i.Category.CategoryName : null,
                        DataCenterName = i.DataCenter != null ? i.DataCenter.Name : null
                    })
                    .ToListAsync();

                return Ok(new
                {
                    DateRange = new { Start = start, End = end },
                    TotalItems = totalItems,
                    ItemsInDateRange = itemsInRange,
                    SampleItems = sampleItems
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // =====================================
        // PRIVATE METHODS
        // =====================================
        private byte[] GenerateReportPdf(List<Item> items, DateTime startDate, DateTime endDate)
        {
            try
            {
                // ✅ DIPERBAIKI: Set QuestPDF license dan font configuration
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
                QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false; // Disable font check

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                        page.Margin(25);

                        // ✅ DIPERBAIKI: Set default font
                        page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(10));

                        // Header
                        page.Header()
                            .AlignCenter()
                            .Text($"Inventory Report\n{startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}")
                            .SemiBold()
                            .FontSize(16);

                        // Content
                        page.Content().PaddingTop(15).Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);  // No
                                columns.RelativeColumn(2);  // Item Code
                                columns.RelativeColumn(3);  // Item Name
                                columns.RelativeColumn(2);  // Category
                                columns.RelativeColumn(2);  // Data Center
                                columns.RelativeColumn(1);  // Qty
                                columns.RelativeColumn(2);  // Status
                                columns.RelativeColumn(2);  // Date
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Text("#").SemiBold();
                                header.Cell().Text("Item Code").SemiBold();
                                header.Cell().Text("Item Name").SemiBold();
                                header.Cell().Text("Category").SemiBold();
                                header.Cell().Text("Data Center").SemiBold();
                                header.Cell().Text("Qty").SemiBold();
                                header.Cell().Text("Status").SemiBold();
                                header.Cell().Text("Created").SemiBold();
                            });

                            // Data rows
                            int index = 1;
                            foreach (var item in items)
                            {
                                table.Cell().Text(index++.ToString());
                                table.Cell().Text(item.ItemCode ?? "-");
                                table.Cell().Text(item.ItemName ?? "-");
                                table.Cell().Text(item.Category?.CategoryName ?? "-");
                                table.Cell().Text(item.DataCenter?.Name ?? "-");
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text(item.Status ?? "-");
                                table.Cell().Text(item.CreatedAt.ToString("dd/MM/yyyy"));
                            }
                        });

                        // Footer
                        page.Footer()
                            .AlignCenter()
                            .Text($"Generated at {DateTime.Now:dd MMM yyyy HH:mm} | Total Items: {items.Count}")
                            .FontSize(10);
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Generation Error: {ex.Message}");
                throw new Exception($"Failed to generate PDF: {ex.Message}", ex);
            }
        }

        private byte[] GenerateReportExcel(List<Item> items, DateTime startDate, DateTime endDate)
        {
            try
            {
                // ✅ PERBAIKI: EPPlus 8+ menggunakan ExcelPackage.License
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Inventory Report");

                // Title
                worksheet.Cells["A1:H1"].Merge = true;
                worksheet.Cells["A1"].Value = $"Inventory Report ({startDate:dd MMM yyyy} - {endDate:dd MMM yyyy})";
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Headers
                worksheet.Cells[3, 1].Value = "No";
                worksheet.Cells[3, 2].Value = "Item Code";
                worksheet.Cells[3, 3].Value = "Item Name";
                worksheet.Cells[3, 4].Value = "Category";
                worksheet.Cells[3, 5].Value = "Data Center";
                worksheet.Cells[3, 6].Value = "Quantity";
                worksheet.Cells[3, 7].Value = "Status";
                worksheet.Cells[3, 8].Value = "Created Date";

                // Style headers
                using (var range = worksheet.Cells[3, 1, 3, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }

                // Data
                int row = 4;
                int no = 1;
                foreach (var item in items)
                {
                    worksheet.Cells[row, 1].Value = no++;
                    worksheet.Cells[row, 2].Value = item.ItemCode ?? "-";
                    worksheet.Cells[row, 3].Value = item.ItemName ?? "-";
                    worksheet.Cells[row, 4].Value = item.Category?.CategoryName ?? "-";
                    worksheet.Cells[row, 5].Value = item.DataCenter?.Name ?? "-";
                    worksheet.Cells[row, 6].Value = item.Quantity;
                    worksheet.Cells[row, 7].Value = item.Status ?? "-";
                    worksheet.Cells[row, 8].Value = item.CreatedAt.ToString("dd/MM/yyyy");

                    // Add borders to data rows
                    using (var range = worksheet.Cells[row, 1, row, 8])
                    {
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Add summary
                worksheet.Cells[row + 2, 1].Value = $"Total Items: {items.Count}";
                worksheet.Cells[row + 3, 1].Value = $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}";

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excel Generation Error: {ex.Message}");
                throw new Exception($"Failed to generate Excel: {ex.Message}", ex);
            }
        }
    }
}