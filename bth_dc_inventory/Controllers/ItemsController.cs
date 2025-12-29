using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.Models;

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

        //GET: api/Items
        // Menampilkan semua barang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            var items = await _context.Items
                                       .Include(i => i.Category)
                                       .Include(i => i.DataCenter)
                                       .Include(i => i.CreatedBy)
                                       .ToListAsync();
            return Ok(items);
        }

        //GET : api/Items/5
        //Menampilkan detail barang bedasarkan ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var item = await _context.Items
                                     .Include(i => i.Category)
                                     .Include(i => i.DataCenter)
                                     .Include (i => i.CreatedBy)
                                     .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        //POST : api/Items
        // Tambah barang baru
        [HttpPost]
        public async Task<ActionResult<Item>> CreatedItem([FromBody] Item item)
        {
            //pastiin validasi data nya sukses
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            item.CreatedAt = DateTime.UtcNow;

            _context.Items.Add(item);   
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);

        }

        // PUT : api/Items/5
        // Mengedit barang bedasarkan ID
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] Item item)
        {
            if (id != item.Id)
            {
                return BadRequest("Provide ID does not match item ID");
            }

            //Singkronisasi objek dengan database 
            var exsistingItem = await _context.Items.FindAsync(id);
            if (exsistingItem == null)
            {
                return NotFound();
            }

            //update semua properti
            exsistingItem.ItemName = item.ItemName;
            exsistingItem.CategoryId = item.CategoryId;
            exsistingItem.DataCenterId = item.DataCenterId;
            exsistingItem.BuyingPrice = item.BuyingPrice;
            exsistingItem.Quantity = item.Quantity; 
            exsistingItem.Status    = item.Status;  
            exsistingItem.DateOfPurchase = item.DateOfPurchase;
            exsistingItem.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!ItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        //DELETE : api.Items/5
        //hapus barang bedasarkan ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //filter pencarian data
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Item>>> FilterItems(

            [FromQuery] int? categoryId,
            [FromQuery] string? itemName,
            [FromQuery] string? dataCenter)
        {
            var query = _context.Items
                                .Include(i => i.Category)
                                .Include(i => i.DataCenter)
                                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(itemName))
            {
                query = query.Where(i => i.ItemName.Contains(itemName));
            }

            if (!string.IsNullOrEmpty(dataCenter))
            {
                query = query.Where(i => i.DataCenter.LocationDetail.Contains(dataCenter));
            }

            var filteredItems = await query.ToListAsync();
            return Ok(filteredItems);
        }


        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }


    }
}
