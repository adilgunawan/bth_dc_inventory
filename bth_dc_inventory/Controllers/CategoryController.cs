using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.Models;
using bth_dc_inventory.DTOs.Category;

namespace bth_dc_inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================
        // GET: api/category
        // =====================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetCategories() 
        {
            var categories = await _context.Categories
                .Select(c => new CategoryReadDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    TotalItems = c.Items.Count()
                })
                .ToListAsync();

            return Ok(categories);
        }

        // =====================================
        // GET: api/category/{id}
        // =====================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryReadDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryReadDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    TotalItems = c.Items.Count()
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound(new { message = "Kategori tidak ditemukan" });

            return Ok(category);
        }

        // =====================================
        // POST: api/category
        // =====================================
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                CategoryName = dto.CategoryName,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, null);
        }

        // =====================================
        // PUT: api/category/{id}
        // =====================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID URL tidak sama dengan ID body");

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Kategori tidak ditemukan");

            category.CategoryName = dto.CategoryName;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =====================================
        // DELETE: api/category/{id}
        // =====================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Kategori tidak ditemukan");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using bth_dc_inventory.Data;
//using bth_dc_inventory.Models;

//namespace bth_dc_inventory.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class CategoryController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public CategoryController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // GET: api/Category
//        // Mendapatkan semua kategori
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
//        {
//            var categories = await _context.Categories.ToListAsync();
//            return Ok(categories);
//        }

//        // GET: api/Category/{id}
//        // Mendapatkan detail kategori berdasarkan ID
//        [HttpGet("{id:int}")]
//        public async Task<ActionResult<Category>> GetCategory(int id)
//        {
//            var category = await _context.Categories.FindAsync(id);

//            if (category == null)
//            {
//                return NotFound(new { message = "Kategori tidak ditemukan." });
//            }

//            return Ok(category);
//        }

//        // POST: api/Category
//        // Menambahkan kategori baru
//        [HttpPost]
//        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            category.CreatedAt = DateTime.UtcNow; // Set waktu pembuatan kategori
//            _context.Categories.Add(category);
//            await _context.SaveChangesAsync();

//            // Redirect ke endpoint GetCategory setelah sukses menambahkan data
//            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
//        }

//        // PUT: api/Category/{id}
//        // Memperbarui kategori berdasarkan ID
//        [HttpPut("{id:int}")]
//        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
//        {
//            if (id != category.Id)
//            {
//                return BadRequest(new { message = "ID pada URL tidak sesuai dengan ID pada data." });
//            }

//            var existingCategory = await _context.Categories.FindAsync(id);
//            if (existingCategory == null)
//            {
//                return NotFound(new { message = "Kategori tidak ditemukan." });
//            }

//            // Perbarui properti yang dapat diubah
//            existingCategory.CategoryName = category.CategoryName;
//            existingCategory.Description = category.Description;
//            existingCategory.Image = category.Image; // Perbarui gambar kategori
//            existingCategory.UpdatedAt = DateTime.UtcNow;

//            try
//            {
//                await _context.SaveChangesAsync();
//                return NoContent(); // Berhasil diperbarui tanpa konten
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!CategoryExists(id))
//                {
//                    return NotFound(new { message = "Kategori tidak ditemukan." });
//                }
//                else
//                {
//                    throw; // Lempar error lain jika bukan karena ID hilang
//                }
//            }
//        }

//        // DELETE: api/Category/{id}
//        // Menghapus kategori berdasarkan ID
//        [HttpDelete("{id:int}")]
//        public async Task<IActionResult> DeleteCategory(int id)
//        {
//            var category = await _context.Categories.FindAsync(id);

//            if (category == null)
//            {
//                return NotFound(new { message = "Kategori tidak ditemukan." });
//            }

//            _context.Categories.Remove(category);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        // Helper method: memeriksa apakah kategori dengan ID tertentu ada
//        private bool CategoryExists(int id)
//        {
//            return _context.Categories.Any(c => c.Id == id);
//        }
//    }
//}