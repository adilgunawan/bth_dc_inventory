using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.DTOs.Report;
using bth_dc_inventory.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Globalization;
using QuestPDF.Drawing;
using QuestPDF.Helpers;


namespace bth_dc_inventory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET REPORT ITEMS (FILTER + PAGINATION)
        // =========================================
        [HttpGet("items")]
        public async Task<IActionResult> GetItemReport(
            [FromQuery] ReportFilterDto filter,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.DataCenter)
                .AsQueryable();

            if (filter.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == filter.CategoryId);

            if (filter.DataCenterId.HasValue)
                query = query.Where(i => i.DataCenterId == filter.DataCenterId);

            if (!string.IsNullOrEmpty(filter.Search))
                query = query.Where(i => i.ItemName.Contains(filter.Search));

            if (filter.StartDate.HasValue)
                query = query.Where(i => i.DateOfPurchase >= filter.StartDate);

            if (filter.EndDate.HasValue)
                query = query.Where(i => i.DateOfPurchase <= filter.EndDate);

            var totalItems = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new ItemReportDto
                {
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    CategoryName = i.Category.CategoryName,
                    DataCenterName = i.DataCenter.Name,
                    Quantity = i.Quantity,
                    BuyingPrice = i.BuyingPrice,
                    DateOfPurchase = i.DateOfPurchase ?? DateTime.MinValue,
                    Status = i.Status
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                data
            });
        }

        // =========================================
        // GET SUMMARY REPORT
        // =========================================
        [HttpGet("summary")]
        public async Task<ActionResult<StockSummaryReportDto>> GetSummary()
        {
            var summary = new StockSummaryReportDto
            {
                TotalItems = await _context.Items.CountAsync(),
                TotalQuantity = await _context.Items.SumAsync(i => i.Quantity),
                TotalAssetValue = await _context.Items.SumAsync(i => i.Quantity * i.BuyingPrice)
            };

            return Ok(summary);
        }

        // =========================================
        // EXPORT REPORT (PDF / EXCEL)
        // =========================================
        [HttpGet("export")]
        public async Task<IActionResult> ExportReport(int month, int year, string format = "pdf")
        {
            var items = await _context.Items
                .Include(i => i.Category)
                .Where(i => i.DateOfPurchase.HasValue &&
                            i.DateOfPurchase.Value.Month == month &&
                            i.DateOfPurchase.Value.Year == year)
                .ToListAsync();

            if (!items.Any())
                return NotFound("Data tidak ditemukan.");

            if (format.ToLower() == "pdf")
                return File(GeneratePdf(items, month, year), "application/pdf",
                    $"Report_{month}_{year}.pdf");

            if (format.ToLower() == "excel")
                return File(GenerateExcel(items),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Report_{month}_{year}.xlsx");

            return BadRequest("Format tidak didukung.");
        }

        // =========================================
        // PDF GENERATOR
        // =========================================
        private byte[] GeneratePdf(IEnumerable<Item> items, int month, int year)
        {
            // Wajib untuk versi QuestPDF baru
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header()
                        .AlignCenter()
                        .Text($"Laporan Inventaris {month}/{year}")
                        .SemiBold()
                        .FontSize(16);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Text("Item").SemiBold();
                            h.Cell().Text("Category").SemiBold();
                            h.Cell().Text("Qty").SemiBold();
                            h.Cell().Text("Price").SemiBold();
                        });

                        foreach (var i in items)
                        {
                            table.Cell().Text(i.ItemName ?? "-");
                            table.Cell().Text(i.Category?.CategoryName ?? "-");
                            table.Cell().Text(i.Quantity.ToString());
                            table.Cell().Text(
                                i.BuyingPrice.ToString("C", CultureInfo.GetCultureInfo("id-ID"))
                            );
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated at: {DateTime.Now:dd MMM yyyy HH:mm}");
                });
            });

            return doc.GeneratePdf();
        }

        //private byte[] GeneratePdf(IEnumerable<Item> items, int month, int year)
        //{
        //    QuestPDF.Settings.License = LicenseType.Community;

        //    var doc = Document.Create(container =>
        //    {
        //        container.Page(page =>
        //        {
        //            page.Margin(20);

        //            page.Footer()
        //            .AlignCenter()
        //            .Text($"Generated at: {DateTime.Now:dd MMM yyyy HH:mm}");

        //            page.Content().Table(table =>
        //            {
        //                table.ColumnsDefinition(c =>
        //                {
        //                    c.RelativeColumn(2);
        //                    c.RelativeColumn(2);
        //                    c.RelativeColumn(1);
        //                    c.RelativeColumn(2);
        //                });

        //                table.Header(h =>
        //                {
        //                    h.Cell().Text("Item").SemiBold();
        //                    h.Cell().Text("Category").SemiBold();
        //                    h.Cell().Text("Qty").SemiBold();
        //                    h.Cell().Text("Price").SemiBold();
        //                });

        //                foreach (var i in items)
        //                {
        //                    table.Cell().Text(i.ItemName);
        //                    table.Cell().Text(i.Category.CategoryName);
        //                    table.Cell().Text(i.Quantity.ToString());
        //                    table.Cell().Text(i.BuyingPrice.ToString("C", CultureInfo.GetCultureInfo("id-ID")));
        //                }
        //            });

        //            page.Footer()
        //                .AlignCenter()
        //                .Text(x =>
        //                {
        //                    x.Span("Generated at: ");
        //                    x.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm"));
        //                });
        //        });
        //    });

        //    return doc.GeneratePdf();
        //}


        // =========================================
        // EXCEL GENERATOR
        // =========================================
        private MemoryStream GenerateExcel(IEnumerable<Item> items)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Report");

            ws.Cell(1, 1).Value = "Item";
            ws.Cell(1, 2).Value = "Category";
            ws.Cell(1, 3).Value = "Qty";
            ws.Cell(1, 4).Value = "Price";

            int row = 2;
            foreach (var i in items)
            {
                ws.Cell(row, 1).Value = i.ItemName;
                ws.Cell(row, 2).Value = i.Category.CategoryName;
                ws.Cell(row, 3).Value = i.Quantity;
                ws.Cell(row, 4).Value = i.BuyingPrice;
                row++;
            }

            var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}


