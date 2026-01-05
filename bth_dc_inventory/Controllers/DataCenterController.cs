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


//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using bth_dc_inventory.Data;
//using bth_dc_inventory.Models;
//using NuGet.Packaging.Signing;


//namespace bth_dc_inventory.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class DataCenterController : ControllerBase
//    {

//       private readonly ApplicationDbContext _context;

//        public DataCenterController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // GET : api/DataCenter
//        //mendapatkan semua data center
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<DataCenter>>> GetDataCenters()
//        {
//            return await _context.DataCenters.ToListAsync();
//        }


//        //GET : api/DataCenter/5
//        // mendapatkan detail data center bedasarkan ID
//        [HttpGet("{id}")]
//        public async Task<ActionResult<DataCenter>> GetDataCenter(int id)
//        {
//            var dataCenter = await _context.DataCenters.FindAsync(id);

//            if (dataCenter == null)
//            {
//                return NotFound();
//            }

//            return dataCenter;
//        }


//        //POST : api/DataCenter
//        //Menambahkan Data Center baru
//        [HttpPost]
//        public async Task<ActionResult<DataCenter>> CreateDataCenter([FromBody] DataCenter dataCenter)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            dataCenter.CreatedAt = DateTime.UtcNow;
//            _context.DataCenters.Add(dataCenter);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetDataCenter), new { id = dataCenter.Id }, dataCenter);

//        }

//        //PUT : api/DataCenter/5
//        //mengupdate data center bedasarkan id
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateDataCenter(int id, [FromBody] DataCenter dataCenter)
//        {
//            if ( id != dataCenter.Id)
//            {
//                return BadRequest("ID yang di berikan tidak cocok dengan ID di entitas");
//            }

//            var existingDataCenter = await _context.DataCenters.FindAsync(id);
//            if (existingDataCenter == null)
//            {
//                return NotFound();
//            }

//            existingDataCenter.Name = dataCenter.Name;
//            existingDataCenter.LocationDetail = dataCenter.LocationDetail;
//            existingDataCenter.ManagerName  = dataCenter.ManagerName;
//            existingDataCenter.UpdatedAt = DateTime.UtcNow;

//            try
//            {
//                await _context.SaveChangesAsync();
//            }

//            catch (DbUpdateConcurrencyException)
//            {
//                if (!DataCenterExists(id))
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

//        // Metode untuk memeriksa apakah DataCenter dengan ID tertentu ada
//        private bool DataCenterExists(int id)
//        {
//            return _context.DataCenters.Any(dc => dc.Id == id);
//        }


//    }
//}
