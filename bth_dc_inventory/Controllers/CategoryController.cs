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
        private readonly IWebHostEnvironment _environment;

        public CategoryController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =====================================
        // GET: api/Category
        // =====================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Items)
                .Select(c => new CategoryReadDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description ?? "",
                    Image = c.Image, // ✅ Include image path
                    TotalItems = c.Items.Count(),
            
                })
                .ToListAsync();

            return Ok(categories);
        }

        // =====================================
        // GET: api/Category/{id}
        // =====================================
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryReadDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            var categoryDto = new CategoryReadDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description ?? "",
                Image = category.Image, // ✅ Include image path
                TotalItems = category.Items.Count(),
           
            };

            return Ok(categoryDto);
        }

        // =====================================
        // POST: api/Category
        // =====================================
        [HttpPost]
        public async Task<ActionResult<CategoryReadDto>> CreateCategory([FromForm] CategoryCreateDto categoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if category name already exists
                if (await _context.Categories.AnyAsync(c => c.CategoryName == categoryDto.CategoryName))
                    return BadRequest("Category name already exists");

                string? imagePath = null;

                // ✅ Handle image upload
                if (categoryDto.Image != null && categoryDto.Image.Length > 0)
                {
                    imagePath = await SaveImageAsync(categoryDto.Image);
                }

                var category = new Category
                {
                    CategoryName = categoryDto.CategoryName,
                    Description = categoryDto.Description,
                    Image = imagePath, // ✅ Save image path
                    CreatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var result = new CategoryReadDto
                {
                    Id = category.Id,
                    CategoryName = category.CategoryName,
                    Description = category.Description ?? "",
                    Image = category.Image,
                    TotalItems = 0,
                
                };

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // =====================================
        // PUT: api/Category/{id}
        // =====================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] CategoryUpdateDto categoryDto)
        {
            try
            {
                if (id != categoryDto.Id)
                    return BadRequest("ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound();

                // Check if category name already exists (excluding current category)
                if (await _context.Categories.AnyAsync(c => c.CategoryName == categoryDto.CategoryName && c.Id != id))
                    return BadRequest("Category name already exists");

                // ✅ Handle image update
                if (categoryDto.Image != null && categoryDto.Image.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(category.Image))
                    {
                        DeleteImage(category.Image);
                    }

                    // Save new image
                    category.Image = await SaveImageAsync(categoryDto.Image);
                }

                category.CategoryName = categoryDto.CategoryName;
                category.Description = categoryDto.Description;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // =====================================
        // DELETE: api/Category/{id}
        // =====================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound();

                if (category.Items.Any())
                    return BadRequest("Cannot delete category with existing products");

                // ✅ Delete associated image
                if (!string.IsNullOrEmpty(category.Image))
                {
                    DeleteImage(category.Image);
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // =====================================
        // GET: api/Category/dropdown
        // =====================================
        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategoryDropdown()
        {
            var categories = await _context.Categories
                .Select(c => new { c.Id, c.CategoryName })
                .ToListAsync();

            return Ok(categories);
        }

        // =====================================
        // PRIVATE METHODS - IMAGE HANDLING
        // =====================================
        private async Task<string> SaveImageAsync(IFormFile image)
        {
            try
            {
                // Create uploads directory if it doesn't exist
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "categories");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Return relative path for database storage
                return Path.Combine("uploads", "categories", uniqueFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving image: {ex.Message}");
            }
        }

        private void DeleteImage(string imagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullPath = Path.Combine(_environment.WebRootPath, imagePath.Replace("/", "\\"));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - image deletion shouldn't break the operation
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }
    }
}