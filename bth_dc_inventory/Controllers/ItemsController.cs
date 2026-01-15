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
                    CategoryName = i.Category.CategoryName,
                    DataCenterName = i.DataCenter.Name,
                    BuyingPrice = i.BuyingPrice,
                    Quantity = i.Quantity,
                    Status = i.Status,
                    DateOfPurchase = i.DateOfPurchase ?? DateTime.MinValue,
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
                    CategoryName = i.Category.CategoryName,
                    DataCenterName = i.DataCenter.Name,
                    BuyingPrice = i.BuyingPrice,
                    Quantity = i.Quantity,
                    Status = i.Status,
                    DateOfPurchase = i.DateOfPurchase ?? DateTime.MinValue,
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

            // =========================
            // VALIDASI FK
            // =========================
            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
                return BadRequest("Invalid CategoryId");

            if (!await _context.DataCenters.AnyAsync(d => d.Id == dto.DataCenterId))
                return BadRequest("Invalid DataCenterId");

            // =========================
            // VALIDASI USER
            // =========================
            var userId = await _context.Users
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId == 0) 
                return BadRequest("No user found. Please create a user first.");

            // =========================
            // CREATE ITEM
            // =========================
            var item = new Item
            {
                ItemCode = dto.ItemCode,
                ItemName = dto.ItemName,
                CategoryId = dto.CategoryId,
                DataCenterId = dto.DataCenterId,
                BuyingPrice = dto.BuyingPrice,
                Quantity = 0,
                Status = "active",
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

        [HttpGet("filter")]
        public async Task<ActionResult<PagedResponseDto<ItemReadDto>>> FilterItems(
    [FromQuery] ItemFilterDto filter)
        {
            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.DataCenter)
                .AsQueryable();

            //  SEARCH
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(i =>
                    i.ItemName.Contains(filter.Search) ||
                    i.ItemCode.Contains(filter.Search));
            }

            //  FILTER CATEGORY
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == filter.CategoryId);
            }

            //  FILTER DATA CENTER
            if (filter.DataCenterId.HasValue)
            {
                query = query.Where(i => i.DataCenterId == filter.DataCenterId);
            }

            var totalItems = await query.CountAsync();

            //  PAGINATION
            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
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
                    DateOfPurchase = i.DateOfPurchase ?? DateTime.MinValue,
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

//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using bth_dc_inventory.Data;
//using bth_dc_inventory.Models;

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

//        //GET: api/Items
//        // Menampilkan semua barang
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
//        {
//            var items = await _context.Items
//                                       .Include(i => i.Category)
//                                       .Include(i => i.DataCenter)
//                                       .Include(i => i.CreatedBy)
//                                       .ToListAsync();
//            return Ok(items);
//        }

//        //GET : api/Items/5
//        //Menampilkan detail barang bedasarkan ID
//        [HttpGet("{id}")]
//        public async Task<ActionResult<Item>> GetItem(int id)
//        {
//            var item = await _context.Items
//                                     .Include(i => i.Category)
//                                     .Include(i => i.DataCenter)
//                                     .Include (i => i.CreatedBy)
//                                     .FirstOrDefaultAsync(i => i.Id == id);

//            if (item == null)
//            {
//                return NotFound();
//            }

//            return Ok(item);
//        }

//        //POST : api/Items
//        // Tambah barang baru
//        [HttpPost]
//        public async Task<ActionResult<Item>> CreatedItem([FromBody] Item item)
//        {
//            //pastiin validasi data nya sukses
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            item.CreatedAt = DateTime.UtcNow;

//            _context.Items.Add(item);   
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);

//        }

//        // PUT : api/Items/5
//        // Mengedit barang bedasarkan ID
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateItem(int id, [FromBody] Item item)
//        {
//            if (id != item.Id)
//            {
//                return BadRequest("Provide ID does not match item ID");
//            }

//            //Singkronisasi objek dengan database 
//            var exsistingItem = await _context.Items.FindAsync(id);
//            if (exsistingItem == null)
//            {
//                return NotFound();
//            }

//            //update semua properti
//            exsistingItem.ItemName = item.ItemName;
//            exsistingItem.CategoryId = item.CategoryId;
//            exsistingItem.DataCenterId = item.DataCenterId;
//            exsistingItem.BuyingPrice = item.BuyingPrice;
//            exsistingItem.Quantity = item.Quantity; 
//            exsistingItem.Status    = item.Status;  
//            exsistingItem.DateOfPurchase = item.DateOfPurchase;
//            exsistingItem.UpdatedAt = DateTime.UtcNow;

//            try
//            {
//                await _context.SaveChangesAsync();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if(!ItemExists(id))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }
//            return NoContent();
//        }

//        //DELETE : api.Items/5
//        //hapus barang bedasarkan ID
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteItem(int id)
//        {
//            var item = await _context.Items.FindAsync(id);

//            if (item == null)
//            {
//                return NotFound();
//            }

//            _context.Items.Remove(item);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        //filter pencarian data
//        [HttpGet("filter")]
//        public async Task<ActionResult<IEnumerable<Item>>> FilterItems(

//            [FromQuery] int? categoryId,
//            [FromQuery] string? itemName,
//            [FromQuery] string? dataCenter)
//        {
//            var query = _context.Items
//                                .Include(i => i.Category)
//                                .Include(i => i.DataCenter)
//                                .AsQueryable();

//            if (categoryId.HasValue)
//            {
//                query = query.Where(i => i.CategoryId == categoryId);
//            }

//            if (!string.IsNullOrEmpty(itemName))
//            {
//                query = query.Where(i => i.ItemName.Contains(itemName));
//            }

//            if (!string.IsNullOrEmpty(dataCenter))
//            {
//                query = query.Where(i => i.DataCenter.LocationDetail.Contains(dataCenter));
//            }

//            var filteredItems = await query.ToListAsync();
//            return Ok(filteredItems);
//        }


//        private bool ItemExists(int id)
//        {
//            return _context.Items.Any(e => e.Id == id);
//        }


//    }
//}
