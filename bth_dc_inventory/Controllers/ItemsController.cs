using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.Models;
using bth_dc_inventory.DTOs.Item;
using bth_dc_inventory.DTOs.Common;
using QuestPDF.Fluent;
using OfficeOpenXml;

namespace bth_dc_inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================
        // GET: api/items
        // =====================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemReadDto>>> GetItems()
        {
            var items = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.DataCenter)
                .Select(i => new ItemReadDto
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,

                    AssetNumber = i.AssetNumber,
                    SerialNumber = i.SerialNumber,
                    //PONumber = i.PONumber,

                    CategoryName = i.Category.CategoryName,
                    DataCenterName = i.DataCenter.Name,
                    BuyingPrice = i.BuyingPrice,
                    Quantity = i.Quantity,
                    Status = i.Status,
                    DateOfPurchase = i.DateOfPurchase,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // =====================================
        // GET: api/items/{id}
        // =====================================

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetItem(int id)
        {
            try
            {
                var item = await _context.Items
                    .Include(i => i.Category)
                    .Include(i => i.DataCenter)
                    .Include(i => i.CreatedBy)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                    return NotFound();

                var result = new
                {
                    Id = item.Id,
                    ItemCode = item.ItemCode ?? "",
                    ItemName = item.ItemName ?? "",
                    AssetNumber = item.AssetNumber ?? "",
                    SerialNumber = item.SerialNumber ?? "",
                    CategoryId = item.CategoryId,
                    CategoryName = item.Category?.CategoryName ?? "",
                    CategoryImage = item.Category?.Image, // ✅ Tambahkan ini
                    DataCenterId = item.DataCenterId,
                    DataCenterName = item.DataCenter?.Name ?? "",
                    BuyingPrice = item.BuyingPrice,
                    Quantity = item.Quantity,
                    Status = item.Status ?? "",
                    DateOfPurchase = item.DateOfPurchase,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt,
                    CreatedByName = item.CreatedBy?.Username ?? ""
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching item", error = ex.Message });
            }
        }

        // =====================================
        // POST: api/items
        // =====================================

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] ItemCreateDto dto)
        {
            // Validasi Model State
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed.", errors });
            }

            // Validasi `CategoryId`
            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            {
                return BadRequest(new { message = "Invalid CategoryId." });
            }

            // Validasi `DataCenterId`
            if (!await _context.DataCenters.AnyAsync(d => d.Id == dto.DataCenterId))
            {
                return BadRequest(new { message = "Invalid DataCenterId." });
            }

            // Get `UserId` untuk atribut `CreatedById`
            var userId = await _context.Users.Select(u => u.Id).FirstOrDefaultAsync();
            if (userId == 0)
            {
                return BadRequest(new { message = "No user found. Please create a user first." });
            }

            // Membuat Item baru
            var item = new Item
            {
                ItemCode = dto.ItemCode,
                ItemName = dto.ItemName,
                AssetNumber = dto.AssetNumber,
                SerialNumber = dto.SerialNumber,
                CategoryId = dto.CategoryId,
                DataCenterId = dto.DataCenterId,
                BuyingPrice = dto.BuyingPrice,
                Quantity = dto.Quantity,
                Status = "Pending",
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                DateOfPurchase = dto.DateOfPurchase,
            };

            try
            {
                // Simpan Item ke database
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the product.", error = ex.Message });
            }

            // Jika sukses, kembalikan respons 201 Created dengan `ItemId`
            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, new
            {
                message = "Item successfully created.",
                itemId = item.Id
            });
        }
       
        // =====================================
        // PUT: api/items/{id}
        // =====================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, ItemUpdateDto dto)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound();

            item.ItemCode = dto.ItemCode;
            item.ItemName = dto.ItemName;

            item.AssetNumber = dto.AssetNumber;
            item.SerialNumber = dto.SerialNumber;
            //item.PONumber = dto.PONumber;

            item.CategoryId = dto.CategoryId;
            item.DataCenterId = dto.DataCenterId;
            item.BuyingPrice = dto.BuyingPrice;
            item.Quantity = dto.Quantity;
            item.Status = dto.Status;
            item.DateOfPurchase = dto.DateOfPurchase;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =====================================
        // DELETE: api/items/{id}
        // =====================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // =====================================
        // GET: api/items/by-date-range
        // =====================================
        [HttpGet("by-date-range")]
        public async Task<ActionResult<IEnumerable<ItemReadDto>>> GetItemsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest("StartDate tidak boleh lebih besar dari EndDate");

            var items = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.DataCenter)
                .Where(i =>
                    i.DateOfPurchase.HasValue &&
                    i.DateOfPurchase.Value.Date >= startDate.Date &&
                    i.DateOfPurchase.Value.Date <= endDate.Date)
                .OrderBy(i => i.DateOfPurchase)
                .Select(i => new ItemReadDto
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    CategoryName = i.Category.CategoryName,
                    DataCenterName = i.DataCenter.Name,
                    BuyingPrice = i.BuyingPrice,
                    Quantity = i.Quantity,
                    Status = i.Status,
                    DateOfPurchase = i.DateOfPurchase,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        // =====================================
        // GET: api/items/export pdf/date-range
        // =====================================
        [HttpGet("export/date-range")]
        public async Task<IActionResult> ExportItemsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest("StartDate tidak boleh lebih besar dari EndDate");

            var items = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.DataCenter)
                .Where(i =>
                    i.DateOfPurchase.HasValue &&
                    i.DateOfPurchase.Value.Date >= startDate.Date &&
                    i.DateOfPurchase.Value.Date <= endDate.Date)
                .OrderBy(i => i.DateOfPurchase)
                .ToListAsync();

            if (!items.Any())
                return NotFound("Tidak ada data pada range tanggal tersebut");

            var pdfBytes = GenerateInventoryPdf(items, startDate, endDate);

            return File(
                pdfBytes,
                "application/pdf",
                $"Inventory_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf"
            );
        }
        private byte[] GenerateInventoryPdf(
        List<Item> items,
        DateTime startDate,
        DateTime endDate)
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                var document = QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(25);

                        page.Header()
                            .AlignCenter()
                            .Text($"Inventory Report\n{startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}")
                            .SemiBold()
                            .FontSize(16);

                        page.Content().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("#").SemiBold();
                                header.Cell().Text("Item Name").SemiBold();
                                header.Cell().Text("Category").SemiBold();
                                header.Cell().Text("Data Center").SemiBold();
                                header.Cell().Text("Purchase Date").SemiBold();
                            });

                            int index = 1;
                            foreach (var item in items)
                            {
                                table.Cell().Text(index++.ToString());
                                table.Cell().Text(item.ItemName);
                                table.Cell().Text(item.Category?.CategoryName ?? "-");
                                table.Cell().Text(item.DataCenter?.Name ?? "-");
                                table.Cell().Text(item.DateOfPurchase?.ToString("dd MMM yyyy") ?? "-");
                            }
                        });

                        page.Footer()
                            .AlignCenter()
                            .Text($"Generated at {DateTime.Now:dd MMM yyyy HH:mm}")
                            .FontSize(10);
                    });
                });

                return document.GeneratePdf();
            }


        // DATA CENTER PRODUCT
        [HttpGet("data-center/{dataCenterId}")]
        public async Task<ActionResult<List<ItemReadDto>>> GetItemsByDataCenter(int dataCenterId)
        {
            // Validasi apakah DataCenterId valid
            var isValidDataCenter = await _context.DataCenters.AnyAsync(dc => dc.Id == dataCenterId);
            if (!isValidDataCenter)
            {
                return NotFound(new { message = $"Data Center with ID {dataCenterId} not found." });
            }

            // Ambil data item berdasarkan DataCenterId
            var items = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.DataCenter)
                .Where(i => i.DataCenterId == dataCenterId)  // Filter berdasarkan DataCenterId
                .Select(i => new ItemReadDto
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    AssetNumber = i.AssetNumber,
                    SerialNumber = i.SerialNumber,
                    CategoryName = i.Category.CategoryName,
                    DataCenterName = i.DataCenter.Name, // Ambil nama DataCenter
                    BuyingPrice = i.BuyingPrice,
                    Quantity = i.Quantity,
                    Status = i.Status,
                    DateOfPurchase = i.DateOfPurchase,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            // Validasi jika tidak ada data ditemukan
            if (!items.Any())
            {
                return NotFound(new { message = $"No items found in Data Center with ID {dataCenterId}." });
            }

            return Ok(items);
        }

        // =====================================
        // FILTER ITEMS
        // =====================================
        [HttpGet("filter")]
        public async Task<ActionResult<PagedResponseDto<ItemReadDto>>> FilterItems(
            [FromQuery] ItemFilterDto filter)
        {
            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.DataCenter)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(i =>
                    i.ItemName.Contains(filter.Search) ||
                    i.ItemCode.Contains(filter.Search) ||
                    //i.PONumber.Contains(filter.Search) ||
                    (i.AssetNumber != null && i.AssetNumber.Contains(filter.Search)) ||
                    (i.SerialNumber != null && i.SerialNumber.Contains(filter.Search))
                );
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(i => i.CategoryId == filter.CategoryId);

            if (filter.DataCenterId.HasValue)
                query = query.Where(i => i.DataCenterId == filter.DataCenterId);

            //if (!string.IsNullOrEmpty(filter.PONumber))
            //    query = query.Where(i => i.PONumber.Contains(filter.PONumber));

            if (!string.IsNullOrEmpty(filter.AssetNumber))
                query = query.Where(i => i.AssetNumber!.Contains(filter.AssetNumber));

            if (!string.IsNullOrEmpty(filter.SerialNumber))
                query = query.Where(i => i.SerialNumber!.Contains(filter.SerialNumber));

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(i => i.Status == filter.Status);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(i => new ItemReadDto
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,

                    AssetNumber = i.AssetNumber,
                    SerialNumber = i.SerialNumber,
                    //PONumber = i.PONumber,

                    CategoryName = i.Category.CategoryName,
               
                    DataCenterName = i.DataCenter.Name,
                    BuyingPrice = i.BuyingPrice,
                    Quantity = i.Quantity,
                    Status = i.Status,
                    DateOfPurchase = i.DateOfPurchase,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            return Ok(new PagedResponseDto<ItemReadDto>
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize),
                Data = items
            });
        }


        // =====================================
        // FILTER ITEMS BY CATEGORY
        // =====================================
        [HttpGet("filter-by-category")]
        public async Task<ActionResult<List<ItemReadDto>>> FilterByCategory(int categoryId)
        {
            // Filter berdasarkan category ID
            var items = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.DataCenter)
                .Where(i => i.CategoryId == categoryId) // Filter kategori
                .OrderByDescending(i => i.CreatedAt) // Urutkan berdasarkan waktu pembuatan
                .Select(i => new ItemReadDto
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    AssetNumber = i.AssetNumber,
                    SerialNumber = i.SerialNumber,
                    CategoryName = i.Category.CategoryName,
                    DataCenterName = i.DataCenter.Name,
                    BuyingPrice = i.BuyingPrice,
                    Quantity = i.Quantity,
                    Status = i.Status,
                    DateOfPurchase = i.DateOfPurchase,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            // Pastikan apakah data ada
            if (!items.Any())
            {
                return NotFound(new { message = "No items found for the given category ID." });
            }

            // Kembalikan data dalam format JSON
            return Ok(items);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.CategoryName
                })
                .ToListAsync();

            return Ok(categories);
        }
    }
}



