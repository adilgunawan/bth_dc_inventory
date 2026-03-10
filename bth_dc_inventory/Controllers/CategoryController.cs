
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
        // GET: api/Category - ✅ DENGAN AKUMULASI TOTAL PRODUCTS
        // =====================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Items) // ✅ Include Items untuk menghitung total
                .Select(c => new CategoryReadDto
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description ?? "",
                    Image = c.Image,
                    TotalItems = c.Items.Count(), // ✅ Akumulasi total products per category

                })
                .OrderByDescending(c => c.TotalItems) // ✅ Sort by total items descending
                .ToListAsync();

            return Ok(categories);
        }

        // =====================================
        // GET: api/Category/{id} - ✅ DENGAN AKUMULASI TOTAL PRODUCTS
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
                Image = category.Image,
                TotalItems = category.Items.Count(), // ✅ Akumulasi total products

            };

            return Ok(categoryDto);
        }

        // =====================================
        // GET: api/Category/stats - ✅ TAMBAHAN: Category statistics
        // =====================================
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetCategoryStats()
        {
            try
            {
                var categoryStats = await _context.Categories
                    .Include(c => c.Items)
                    .Select(c => new
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName,
                        TotalItems = c.Items.Count(),
                        ActiveItems = c.Items.Count(i => i.Status == "Active"),
                        PendingItems = c.Items.Count(i => i.Status == "Pending"),
                        ArrivedItems = c.Items.Count(i => i.Status == "Arrived"),
                        Image = c.Image
                    })
                    .OrderByDescending(c => c.TotalItems)
                    .ToListAsync();

                var totalCategories = categoryStats.Count;
                var totalProducts = categoryStats.Sum(c => c.TotalItems);
                var averageItems = totalCategories > 0 ? Math.Round((double)totalProducts / totalCategories, 1) : 0;

                return Ok(new
                {
                    TotalCategories = totalCategories,
                    TotalProducts = totalProducts,
                    AverageItems = averageItems,
                    Categories = categoryStats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching category stats", error = ex.Message });
            }
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

                // Handle image upload
                if (categoryDto.Image != null && categoryDto.Image.Length > 0)
                {
                    imagePath = await SaveImageAsync(categoryDto.Image);
                }

                var category = new Category
                {
                    CategoryName = categoryDto.CategoryName,
                    Description = categoryDto.Description,
                    Image = imagePath,
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
                    TotalItems = 0, // New category starts with 0 items

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

                // Handle image update
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

                // ✅ CHECK: Prevent delete if category has products
                if (category.Items.Any())
                    return BadRequest($"Cannot delete category. It contains {category.Items.Count} products. Please move or delete the products first.");

                // Delete associated image
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
                .Select(c => new {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    TotalItems = c.Items.Count() // ✅ Include total items in dropdown
                })
                .OrderBy(c => c.CategoryName)
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

                // ✅ PERBAIKI: Generate nama file yang lebih pendek
                string extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                // Validasi extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("Invalid file type. Only images are allowed.");
                }

                // Generate simple unique filename
                string uniqueFileName = Guid.NewGuid().ToString("N") + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Return relative path for database storage
                return $"uploads/categories/{uniqueFileName}";
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
                    string fullPath = Path.Combine(_environment.WebRootPath, imagePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
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