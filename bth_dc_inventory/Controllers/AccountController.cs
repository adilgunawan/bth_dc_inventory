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

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalItems = await _context.Items.CountAsync();
                var pendingItems = await _context.Items.CountAsync(i => i.Status == "Pending");
                var activeItems = await _context.Items.CountAsync(i => i.Status == "Arrived" || i.Status == "Active");

                var categoryStats = await _context.Categories
                    .Select(c => new
                    {
                        CategoryName = c.CategoryName,
                        ItemCount = c.Items.Count()
                    })
                    .ToListAsync();

                var dataCenterStats = await _context.DataCenters
                    .Select(dc => new
                    {
                        Id = dc.Id,
                        Name = dc.Name,
                        LocationDetail = dc.LocationDetail,
                        ActiveItems = dc.Items.Count(i => i.Status == "Arrived" || i.Status == "Active"),
                        TotalItems = dc.Items.Count()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalItems = totalItems,
                    PendingItems = pendingItems,
                    ActiveItems = activeItems,
                    CategoryStats = categoryStats,
                    DataCenterStats = dataCenterStats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching dashboard stats", error = ex.Message });
            }
        }
    }
}