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
        // GET: api/Category - 
        // =====================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetCategories()
        {
            try
            {
                // ✅ DEBUG: Cek total items di database
                var totalItemsInDb = await _context.Items.CountAsync();
                Console.WriteLine($"=== DEBUG CATEGORY ITEMS ===");
                Console.WriteLine($"Total items in database: {totalItemsInDb}");

                // ✅ DEBUG: Cek items per category dengan raw query
                var itemsPerCategory = await _context.Items
                    .GroupBy(i => i.CategoryId)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .ToListAsync();

                Console.WriteLine("Items per CategoryId:");
                foreach (var item in itemsPerCategory)
                {
                    Console.WriteLine($"  CategoryId: {item.CategoryId}, Count: {item.Count}");
                }

                // ✅ PERBAIKI: Query dengan multiple approaches
                var categories = await _context.Categories
                    .Select(c => new CategoryReadDto
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName,
                        Description = c.Description ?? "",
                        Image = c.Image,
                        // ✅ METHOD 1: Direct count with explicit join
                        TotalItems = _context.Items.Count(i => i.CategoryId == c.Id)
                    })
                    .OrderByDescending(c => c.TotalItems)
                    .ToListAsync();

                // ✅ DEBUG: Log hasil
                Console.WriteLine("Final results:");
                foreach (var cat in categories)
                {
                    Console.WriteLine($"  Category: {cat.CategoryName}, Items: {cat.TotalItems}");
                }

                return Ok(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCategories: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error fetching categories", error = ex.Message });
            }
        }

        // =====================================
        // GET: api/Category/debug - 
        // =====================================
        [HttpGet("debug")]
        public async Task<ActionResult> DebugCategoryItems()
        {
            try
            {
                // ✅ DEBUG: Cek semua data
                var allCategories = await _context.Categories.ToListAsync();
                var allItems = await _context.Items.ToListAsync();

                var debugInfo = new
                {
                    TotalCategories = allCategories.Count,
                    TotalItems = allItems.Count,
                    Categories = allCategories.Select(c => new
                    {
                        c.Id,
                        c.CategoryName,
                        ItemsWithThisCategory = allItems.Count(i => i.CategoryId == c.Id)
                    }).ToList(),
                    ItemsGroupedByCategory = allItems
                        .GroupBy(i => i.CategoryId)
                        .Select(g => new
                        {
                            CategoryId = g.Key,
                            Count = g.Count(),
                            CategoryName = allCategories.FirstOrDefault(c => c.Id == g.Key)?.CategoryName ?? "Unknown"
                        }).ToList(),
                    SampleItems = allItems.Take(5).Select(i => new
                    {
                        i.Id,
                        i.ItemName,
                        i.CategoryId,
                        CategoryName = allCategories.FirstOrDefault(c => c.Id == i.CategoryId)?.CategoryName ?? "Not Found"
                    }).ToList()
                };

                return Ok(debugInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // =====================================
        // GET: api/Category/{id} - ✅ 
        // =====================================
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryReadDto>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound();

                //  HITUNG ITEMS SECARA TERPISAH
                var itemCount = await _context.Items.CountAsync(i => i.CategoryId == id);

                Console.WriteLine($"Category {id} ({category.CategoryName}) has {itemCount} items");

                var categoryDto = new CategoryReadDto
                {
                    Id = category.Id,
                    CategoryName = category.CategoryName,
                    Description = category.Description ?? "",
                    Image = category.Image,
                    TotalItems = itemCount
                };

                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCategory: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching category", error = ex.Message });
            }
        }

        // =====================================
        // GET: api/Category/stats - ✅ 
        // =====================================
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetCategoryStats()
        {
            try
            {
                // ✅ GUNAKAN PENDEKATAN YANG SAMA
                var categories = await _context.Categories.ToListAsync();
                var categoryStats = new List<object>();

                foreach (var category in categories)
                {
                    var totalItems = await _context.Items.CountAsync(i => i.CategoryId == category.Id);
                    var activeItems = await _context.Items.CountAsync(i => i.CategoryId == category.Id && i.Status == "Active");
                    var pendingItems = await _context.Items.CountAsync(i => i.CategoryId == category.Id && i.Status == "Pending");
                    var arrivedItems = await _context.Items.CountAsync(i => i.CategoryId == category.Id && i.Status == "Arrived");

                    categoryStats.Add(new
                    {
                        Id = category.Id,
                        CategoryName = category.CategoryName,
                        TotalItems = totalItems,
                        ActiveItems = activeItems,
                        PendingItems = pendingItems,
                        ArrivedItems = arrivedItems,
                        Image = category.Image
                    });
                }

                var orderedStats = categoryStats.OrderByDescending(c => ((dynamic)c).TotalItems).ToList();
                var totalCategories = categories.Count;
                var totalProducts = categoryStats.Sum(c => ((dynamic)c).TotalItems);
                var averageItems = totalCategories > 0 ? Math.Round((double)totalProducts / totalCategories, 1) : 0;

                return Ok(new
                {
                    TotalCategories = totalCategories,
                    TotalProducts = totalProducts,
                    AverageItems = averageItems,
                    Categories = orderedStats
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCategoryStats: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching category stats", error = ex.Message });
            }
        }

        // =====================================
        // GET: api/Category/dropdown - ✅ 
        // =====================================
        [HttpGet("dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategoryDropdown()
        {
            try
            {
                var categories = await _context.Categories
                    .Select(c => new {
                        Id = c.Id,
                        CategoryName = c.CategoryName,
                        TotalItems = _context.Items.Count(i => i.CategoryId == c.Id) // ✅ Direct count
                    })
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching dropdown", error = ex.Message });
            }
        }

        // ... (rest of your methods remain the same: POST, PUT, DELETE, and private methods)

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound();

                // ✅ CHECK: Count items with explicit query
                var itemCount = await _context.Items.CountAsync(i => i.CategoryId == id);
                if (itemCount > 0)
                    return BadRequest($"Cannot delete category. It contains {itemCount} products. Please move or delete the products first.");

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
        // PRIVATE METHODS - IMAGE HANDLING (same as before)
        // =====================================
        private async Task<string> SaveImageAsync(IFormFile image)
        {
            try
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "categories");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("Invalid file type. Only images are allowed.");
                }

                string uniqueFileName = Guid.NewGuid().ToString("N") + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

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
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }
    }
}