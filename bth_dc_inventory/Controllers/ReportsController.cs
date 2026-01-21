using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.DTOs.Report;
using bth_dc_inventory.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Globalization;

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
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Footer()
                    .AlignCenter()
                    .Text($"Generated at: {DateTime.Now:dd MMM yyyy HH:mm}");

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
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
                            table.Cell().Text(i.ItemName);
                            table.Cell().Text(i.Category.CategoryName);
                            table.Cell().Text(i.Quantity.ToString());
                            table.Cell().Text(i.BuyingPrice.ToString("C", CultureInfo.GetCultureInfo("id-ID")));
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated at: ");
                            x.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm"));
                        });
                });
            });

            return doc.GeneratePdf();
        }
        //private byte[] GeneratePdf(IEnumerable<Item> items, int month, int year)
        //{
        //    var doc = Document.Create(container =>
        //    {
        //        container.Page(page =>
        //        {
        //            page.Content().Table(table =>
        //            {
        //                table.ColumnsDefinition(c =>
        //                {
        //                    c.RelativeColumn();
        //                    c.RelativeColumn();
        //                    c.RelativeColumn();
        //                    c.RelativeColumn();
        //                });

        //                table.Header(h =>
        //                {
        //                    h.Cell().Text("Item");
        //                    h.Cell().Text("Category");
        //                    h.Cell().Text("Qty");
        //                    h.Cell().Text("Price");
        //                });

        //                foreach (var i in items)
        //                {
        //                    table.Cell().Text(i.ItemName);
        //                    table.Cell().Text(i.Category.CategoryName);
        //                    table.Cell().Text(i.Quantity.ToString());
        //                    table.Cell().Text(i.BuyingPrice.ToString("C"));
        //                }
        //            });
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

//using ClosedXML.Excel;
//using QuestPDF.Fluent;
//using QuestPDF.Infrastructure;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using bth_dc_inventory.Data;
//using bth_dc_inventory.Models;
//using System.Globalization;



//namespace bth_dc_inventory.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ReportsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public ReportsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        //GET : api/Report/5
//        //mendapatkan semua laporan
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Report>>> GetReports()
//        {
//            var reports = await _context.Reports.Include(r => r.Category).ToListAsync();
//            return Ok(reports);
//        }


//        //GET : api/Reports/5
//        //Mendapatkan detail laporan bedasarkan ID
//        [HttpGet("{id}")]
//        public async Task<ActionResult<Report>> GetReport(int id)
//        {
//            var report = await _context.Reports
//                .Include(r => r.Category)
//                .FirstOrDefaultAsync(r => r.Id == id);

//            if (report == null)
//            {
//                return NotFound();
//            }

//            return report;

//        }


//        //POST : api/Reports
//        // menambahkan laporan baru 
//        [HttpPost]
//        public async Task<ActionResult<Report>> CreateReport([FromBody] Report report)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            report.GeneratedAt = DateTime.UtcNow;
//            _context.Reports.Add(report);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
//        }


//        //DELETE : api/Reports/5 
//        // Menghapus Laporan Bedasarkan ID 
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteReport(int id)
//        {
//            var report = await _context.Reports.FindAsync(id);

//            if (report == null)
//            {

//                return NotFound();

//            }

//            _context.Reports.Remove(report);
//            await _context.SaveChangesAsync();

//            return NoContent();


//        }

//        // Utility method untuk memeriksa apakah laporan dengan ID tertentu ada di database
//        private bool ReportExists(int id)
//        {
//            return _context.Reports.Any(r => r.Id == id);
//        }

//        // GET: api/Reports/export
//        // Export laporan produk berdasarkan bulan dan tahun
//        [HttpGet("export")]
//        public async Task<IActionResult> ExportReport([FromQuery] int month, [FromQuery] int year, [FromQuery] string format)
//        {
//            // Ambil data dari tabel "Items" berdasarkan periode tertentu
//            var items = await _context.Items
//                .Include(i => i.Category) // Include kategori untuk mendapatkan nama kategori
//                .Where(i => i.DateOfPurchase.HasValue &&
//                            i.DateOfPurchase.Value.Month == month &&
//                            i.DateOfPurchase.Value.Year == year)
//                .ToListAsync();

//            // Jika tidak ada data, kembalikan pesan NotFound
//            if (!items.Any())
//            {
//                return NotFound(new { message = "Data tidak ditemukan untuk periode tersebut." });
//            }

//            // Format laporan: PDF atau Excel
//            format = format.ToLower();
//            if (format == "pdf")
//            {
//                var pdfBytes = GeneratePdfReport(items, month, year);
//                return File(pdfBytes, "application/pdf", $"Laporan_{year}_{month}.pdf");
//            }
//            else if (format == "excel")
//            {
//                var excelStream = GenerateExcelReport(items, month, year);
//                return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Laporan_{year}_{month}.xlsx");
//            }
//            else
//            {
//                return BadRequest(new { Error = "Format tidak didukung! Gunakan format 'pdf' atau 'excel'." });
//            }
//        }

//        // Fungsi untuk menghasilkan file PDF
//        private byte[] GeneratePdfReport(IEnumerable<Item> items, int month, int year)
//        {
//            var document = Document.Create(container =>
//            {
//                container.Page(page =>
//                {
//                    //page.Margin(50);
//                    //page.Header().Text($"Laporan Bulanan: {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}")
//                    //    .FontSize(20).SemiBold().AlignCenter();

//                    page.Content().Table(table =>
//                    {
//                        // Definisikan kolom
//                        table.ColumnsDefinition(columns =>
//                        {
//                            columns.ConstantColumn(50);  // Kolom ID
//                            columns.RelativeColumn();    // Kolom Nama Produk
//                            columns.RelativeColumn();    // Kolom Nama Kategori
//                            columns.RelativeColumn();    // Kolom Harga
//                            columns.RelativeColumn();    // Kolom Quantity
//                        });

//                        // Tambahkan header tabel
//                        table.Header(header =>
//                        {
//                            header.Cell().Element(CellStyle).Text("ID");
//                            header.Cell().Element(CellStyle).Text("Nama Produk");
//                            header.Cell().Element(CellStyle).Text("Kategori");
//                            header.Cell().Element(CellStyle).Text("Harga");
//                            header.Cell().Element(CellStyle).Text("Jumlah");

//                            static IContainer CellStyle(IContainer container)
//                            {
//                                return container.DefaultTextStyle(x => x.SemiBold()).Padding(5).BorderBottom(1.5f);
//                            }
//                        });

//                        // Tambahkan data per baris
//                        foreach (var item in items)
//                        {
//                            table.Cell().Element(CellStyle).Text(item.Id.ToString());
//                            table.Cell().Element(CellStyle).Text(item.ItemName);
//                            table.Cell().Element(CellStyle).Text(item.Category.CategoryName);
//                            table.Cell().Element(CellStyle).Text(item.BuyingPrice.ToString("C", CultureInfo.CurrentCulture));
//                            table.Cell().Element(CellStyle).Text(item.Quantity.ToString());

//                            static IContainer CellStyle(IContainer container)
//                            {
//                                return container.Padding(5).BorderBottom(1);
//                            }
//                        }
//                    });
//                });
//            });

//            return document.GeneratePdf();
//        }

//        // Fungsi untuk menghasilkan file Excel
//        private MemoryStream GenerateExcelReport(IEnumerable<Item> items, int month, int year)
//        {
//            using var workbook = new XLWorkbook();
//            var worksheet = workbook.Worksheets.Add($"Laporan_{year}_{month}");

//            // Tambahkan header tabel
//            var headerRow = 1;
//            worksheet.Cell(headerRow, 1).Value = "ID";
//            worksheet.Cell(headerRow, 2).Value = "Nama Produk";
//            worksheet.Cell(headerRow, 3).Value = "Kategori";
//            worksheet.Cell(headerRow, 4).Value = "Harga";
//            worksheet.Cell(headerRow, 5).Value = "Jumlah";

//            // Tambahkan data per baris
//            var currentRow = 2;
//            foreach (var item in items)
//            {
//                worksheet.Cell(currentRow, 1).Value = item.Id;
//                worksheet.Cell(currentRow, 2).Value = item.ItemName;
//                worksheet.Cell(currentRow, 3).Value = item.Category.CategoryName;
//                worksheet.Cell(currentRow, 4).Value = item.BuyingPrice;
//                worksheet.Cell(currentRow, 5).Value = item.Quantity;
//                currentRow++;
//            }

//            // Simpan workbook ke MemoryStream
//            using var stream = new MemoryStream();
//            workbook.SaveAs(stream);
//            stream.Position = 0; // Pastikan posisi stream berada di awal
//            return stream;
//        }

//    }
//}
