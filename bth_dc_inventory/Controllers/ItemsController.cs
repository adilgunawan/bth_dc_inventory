using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.Models;
using bth_dc_inventory.DTOs.Item;
using bth_dc_inventory.DTOs.Common;

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
        public async Task<ActionResult<ItemReadDto>> GetItem(int id)
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.DataCenter)
                .Where(i => i.Id == id)
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
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // =====================================
        // POST: api/items
        // =====================================
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] ItemCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
                return BadRequest("Invalid CategoryId");

            if (!await _context.DataCenters.AnyAsync(d => d.Id == dto.DataCenterId))
                return BadRequest("Invalid DataCenterId");

            var userId = await _context.Users
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId == 0)
                return BadRequest("No user found. Please create a user first.");

            var item = new Item
            {
                ItemCode = dto.ItemCode,
                ItemName = dto.ItemName,

                AssetNumber = dto.AssetNumber,
                SerialNumber = dto.SerialNumber,
                //PONumber = dto.PONumber,

                CategoryId = dto.CategoryId,
                DataCenterId = dto.DataCenterId,
                BuyingPrice = dto.BuyingPrice,

                Quantity = 0,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, new
            {
                message = "Item successfully created",
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
    }
}

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using bth_dc_inventory.Data;
//using bth_dc_inventory.Models;
//using bth_dc_inventory.DTOs.Item;
//using bth_dc_inventory.DTOs.Common;

//namespace bth_dc_inventory.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ItemsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public ItemsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // =====================================
//        // GET: api/items
//        // =====================================
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<ItemReadDto>>> GetItems()
//        {
//            var items = await _context.Items
//                .Include(i => i.Category)
//                .Include(i => i.DataCenter)
//                .Select(i => new ItemReadDto
//                {
//                    Id = i.Id,
//                    ItemCode = i.ItemCode,
//                    ItemName = i.ItemName,
//                    CategoryName = i.Category.CategoryName,
//                    DataCenterName = i.DataCenter.Name,
//                    BuyingPrice = i.BuyingPrice,
//                    Quantity = i.Quantity,
//                    Status = i.Status,
//                    DateOfPurchase = i.DateOfPurchase ?? DateTime.MinValue,
//                    UpdatedAt = i.UpdatedAt
//                })
//                .ToListAsync();

//            return Ok(items);
//        }

//        // =====================================
//        // GET: api/items/{id}
//        // =====================================
//        [HttpGet("{id}")]
//        public async Task<ActionResult<ItemReadDto>> GetItem(int id)
//        {
//            var item = await _context.Items
//                .Include(i => i.Category)
//                .Include(i => i.DataCenter)
//                .Where(i => i.Id == id)
//                .Select(i => new ItemReadDto
//                {
//                    Id = i.Id,
//                    ItemCode = i.ItemCode,
//                    ItemName = i.ItemName,
//                    CategoryName = i.Category.CategoryName,
//                    DataCenterName = i.DataCenter.Name,
//                    BuyingPrice = i.BuyingPrice,
//                    Quantity = i.Quantity,
//                    Status = i.Status,
//                    DateOfPurchase = i.DateOfPurchase ?? DateTime.MinValue,
//                    UpdatedAt = i.UpdatedAt
//                })
//                .FirstOrDefaultAsync();

//            if (item == null)
//                return NotFound();

//            return Ok(item);
//        }

//        // =====================================
//        // POST: api/items
//        // =====================================
//        [HttpPost]
//        public async Task<IActionResult> CreateItem([FromBody] ItemCreateDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            // =========================
//            // VALIDASI FK
//            // =========================
//            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
//                return BadRequest("Invalid CategoryId");

//            if (!await _context.DataCenters.AnyAsync(d => d.Id == dto.DataCenterId))
//                return BadRequest("Invalid DataCenterId");

//            // =========================
//            // VALIDASI USER
//            // =========================
//            var userId = await _context.Users
//                .Select(u => u.Id)
//                .FirstOrDefaultAsync();

//            if (userId == 0) 
//                return BadRequest("No user found. Please create a user first.");

//            // =========================
//            // CREATE ITEM
//            // =========================
//            var item = new Item
//            {
//                ItemCode = dto.ItemCode,
//                ItemName = dto.ItemName,
//                CategoryId = dto.CategoryId,
//                DataCenterId = dto.DataCenterId,
//                BuyingPrice = dto.BuyingPrice,
//                Quantity = 0,
//                Status = "Pending",
//                CreatedAt = DateTime.UtcNow,
//                CreatedById = userId
//            };


//            _context.Items.Add(item);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, new
//            {
//                message = "Item successfully created",
//                itemId = item.Id
//            });
//        }

//        // =====================================
//        // PUT: api/items/{id}
//        // =====================================
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateItem(int id, ItemUpdateDto dto)
//        {
//            var item = await _context.Items.FindAsync(id);
//            if (item == null)
//                return NotFound();

//            item.ItemCode = dto.ItemCode;
//            item.ItemName = dto.ItemName;
//            item.CategoryId = dto.CategoryId;
//            item.DataCenterId = dto.DataCenterId;
//            item.BuyingPrice = dto.BuyingPrice;
//            item.Quantity = dto.Quantity;
//            item.Status = dto.Status;
//            item.DateOfPurchase = dto.DateOfPurchase;
//            item.UpdatedAt = DateTime.UtcNow;

//            await _context.SaveChangesAsync();
//            return NoContent();
//        }

//        // =====================================
//        // DELETE: api/items/{id}
//        // =====================================
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteItem(int id)
//        {
//            var item = await _context.Items.FindAsync(id);
//            if (item == null)
//                return NotFound();

//            _context.Items.Remove(item);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        // =====================================
//        // FILER PRODUCTS
//        // =====================================
//        [HttpGet("filter")]
//        public async Task<ActionResult<PagedResponseDto<ItemReadDto>>> FilterItems(
//    [FromQuery] ItemFilterDto filter)
//        {
//            var query = _context.Items
//                .Include(i => i.Category)
//                .Include(i => i.DataCenter)
//                .AsQueryable();

//            //  SEARCH
//            if (!string.IsNullOrEmpty(filter.Search))
//            {
//                query = query.Where(i =>
//                    i.ItemName.Contains(filter.Search) ||
//                    i.ItemCode.Contains(filter.Search));
//            }

//            //  FILTER CATEGORY
//            if (filter.CategoryId.HasValue)
//            {
//                query = query.Where(i => i.CategoryId == filter.CategoryId);
//            }

//            //  FILTER DATA CENTER
//            if (filter.DataCenterId.HasValue)
//            {
//                query = query.Where(i => i.DataCenterId == filter.DataCenterId);
//            }

//            var totalItems = await query.CountAsync();

//            //  PAGINATION
//            var items = await query
//                .OrderByDescending(i => i.CreatedAt)
//                .Skip((filter.Page - 1) * filter.PageSize)
//                .Take(filter.PageSize)
//                .Select(i => new ItemReadDto
//                {
//                    Id = i.Id,
//                    ItemCode = i.ItemCode,
//                    ItemName = i.ItemName,
//                    CategoryName = i.Category.CategoryName,
//                    DataCenterName = i.DataCenter.Name,
//                    BuyingPrice = i.BuyingPrice,
//                    Quantity = i.Quantity,
//                    Status = i.Status,
//                    DateOfPurchase = i.DateOfPurchase ?? DateTime.MinValue,
//                    UpdatedAt = i.UpdatedAt
//                })
//                .ToListAsync();

//            return Ok(new PagedResponseDto<ItemReadDto>
//            {
//                Page = filter.Page,
//                PageSize = filter.PageSize,
//                TotalItems = totalItems,
//                TotalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize),
//                Data = items
//            });
//        }
//    }

//}

