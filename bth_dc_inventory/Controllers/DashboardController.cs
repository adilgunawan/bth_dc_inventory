using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;

namespace bth_dc_inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================
        // GET: api/dashboard/dashboard-stats
        // =====================================
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalProducts = await _context.Items.CountAsync();
                var pendingProducts = await _context.Items.CountAsync(i => i.Status == "Pending");
                var activeProducts = await _context.Items.CountAsync(i => i.Status == "Active");
                var arrivedProducts = await _context.Items.CountAsync(i => i.Status == "Arrived");

                return Ok(new
                {
                    TotalProducts = totalProducts,
                    PendingProducts = pendingProducts,
                    ActiveProducts = activeProducts,
                    ArrivedProducts = arrivedProducts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching dashboard stats", error = ex.Message });
            }
        }

        // =====================================
        // GET: api/dashboard/inventory-distribution
        // =====================================
        [HttpGet("inventory-distribution")]
        public async Task<IActionResult> GetInventoryDistribution()
        {
            try
            {
                var categoryStats = await _context.Items
                    .Include(i => i.Category)
                    .Where(i => i.Category != null)
                    .GroupBy(i => i.Category!.CategoryName)
                    .Select(g => new
                    {
                        Label = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                return Ok(categoryStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching inventory distribution", error = ex.Message });
            }
        }

        // =====================================
        // GET: api/dashboard/datacenter-stats
        // =====================================
        [HttpGet("datacenter-stats")]
        public async Task<IActionResult> GetDataCenterStats()
        {
            try
            {
                var dataCenterStats = await _context.DataCenters
                    .Select(dc => new
                    {
                        Id = dc.Id,
                        Name = dc.Name,
                        Location = dc.Name + " Location",
                        ActiveItems = _context.Items.Count(i => i.DataCenterId == dc.Id && i.Status == "Active"),
                        TotalItems = _context.Items.Count(i => i.DataCenterId == dc.Id),
                        Capacity = 300,
                        Status = "Operational"
                    })
                    .ToListAsync();

                return Ok(dataCenterStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching data center stats", error = ex.Message });
            }
        }

        // =====================================
        // GET: api/dashboard/search
        // =====================================
        [HttpGet("search")]
        public async Task<IActionResult> SearchItems([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return Ok(new List<object>());
                }

                var searchResults = await _context.Items
                    .Include(i => i.Category)
                    .Include(i => i.DataCenter)
                    .Where(i =>
                        (i.ItemName != null && i.ItemName.Contains(query)) ||
                        (i.ItemCode != null && i.ItemCode.Contains(query)) ||
                        (i.AssetNumber != null && i.AssetNumber.Contains(query)) ||
                        (i.SerialNumber != null && i.SerialNumber.Contains(query)))
                    .Select(i => new
                    {
                        Id = i.Id,
                        ItemCode = i.ItemCode ?? "",
                        ItemName = i.ItemName ?? "",
                        Category = i.Category != null ? i.Category.CategoryName : "Unknown",
                        DataCenter = i.DataCenter != null ? i.DataCenter.Name : "Unknown",
                        Status = i.Status ?? "Unknown"
                    })
                    .Take(10)
                    .ToListAsync();

                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching items", error = ex.Message });
            }
        }
    }
}