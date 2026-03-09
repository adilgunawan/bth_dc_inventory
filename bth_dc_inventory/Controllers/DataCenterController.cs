using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.DTOs.DataCenter;
using bth_dc_inventory.Models;

namespace bth_dc_inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataCentersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataCentersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================
        // GET: api/datacenters
        // =====================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DataCenterReadDto>>> GetDataCenters()
        {
            var dataCenters = await _context.DataCenters
                .Select(dc => new DataCenterReadDto
                {
                    Id = dc.Id,
                    Name = dc.Name,
                    LocationDetail = dc.LocationDetail,
                    ManagerName = dc.ManagerName,
                    CreatedAt = dc.CreatedAt,
                    UpdatedAt = dc.UpdatedAt,
                    TotalItems = _context.Items.Count(i => i.DataCenterId == dc.Id)
                })
                .ToListAsync();

            return Ok(dataCenters);
        }

        // =====================================
        // GET: api/datacenters/dropdown
        // =====================================
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDataCentersDropdown()
        {
            var dataCenters = await _context.DataCenters
                .Select(dc => new
                {
                    dc.Id,
                    dc.Name
                })
                .ToListAsync();

            return Ok(dataCenters);
        }

        // =====================================
        // GET: api/datacenters/{id}
        // =====================================
        [HttpGet("{id}")]
        public async Task<ActionResult<DataCenterReadDto>> GetDataCenter(int id)
        {
            var dc = await _context.DataCenters
                .Where(dc => dc.Id == id)
                .Select(dc => new DataCenterReadDto
                {
                    Id = dc.Id,
                    Name = dc.Name,
                    LocationDetail = dc.LocationDetail,
                    ManagerName = dc.ManagerName,
                    CreatedAt = dc.CreatedAt,
                    UpdatedAt = dc.UpdatedAt,
                    TotalItems = _context.Items.Count(i => i.DataCenterId == dc.Id)
                })
                .FirstOrDefaultAsync();

            if (dc == null)
                return NotFound();

            return Ok(dc);
        }

        // =====================================
        // POST: api/datacenters
        // =====================================
        [HttpPost]
        public async Task<IActionResult> CreateDataCenter(DataCenterCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dataCenter = new DataCenter
            {
                Name = dto.Name,
                LocationDetail = dto.LocationDetail,
                ManagerName = dto.ManagerName,
                CreatedAt = DateTime.UtcNow
            };

            _context.DataCenters.Add(dataCenter);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDataCenter), new { id = dataCenter.Id }, null);
        }

        // =====================================
        // PUT: api/datacenters/{id}
        // =====================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDataCenter(int id, DataCenterUpdateDto dto)
        {
            var dc = await _context.DataCenters.FindAsync(id);
            if (dc == null)
                return NotFound();

            dc.Name = dto.Name;
            dc.LocationDetail = dto.LocationDetail;
            dc.ManagerName = dto.ManagerName;
            dc.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =====================================
        // DELETE: api/datacenters/{id}
        // =====================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataCenter(int id)
        {
            var dc = await _context.DataCenters.FindAsync(id);
            if (dc == null)
                return NotFound();

            _context.DataCenters.Remove(dc);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}



