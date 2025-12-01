using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.Models;
using NuGet.Packaging.Signing;


namespace bth_dc_inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataCenterController : ControllerBase
    {
        
       private readonly ApplicationDbContext _context;

        public DataCenterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET : api/DataCenter
        //mendapatkan semua data center
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DataCenter>>> GetDataCenters()
        {
            return await _context.DataCenters.ToListAsync();
        }


        //GET : api/DataCenter/5
        // mendapatkan detail data center bedasarkan ID
        [HttpGet("{id}")]
        public async Task<ActionResult<DataCenter>> GetDataCenter(int id)
        {
            var dataCenter = await _context.DataCenters.FindAsync(id);

            if (dataCenter == null)
            {
                return NotFound();
            }

            return dataCenter;
        }


        //POST : api/DataCenter
        //Menambahkan Data Center baru
        [HttpPost]
        public async Task<ActionResult<DataCenter>> CreateDataCenter([FromBody] DataCenter dataCenter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dataCenter.CreatedAt = DateTime.UtcNow;
            _context.DataCenters.Add(dataCenter);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDataCenter), new { id = dataCenter.Id }, dataCenter);
         
        }

        //PUT : api/DataCenter/5
        //mengupdate data center bedasarkan id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDataCenter(int id, [FromBody] DataCenter dataCenter)
        {
            if ( id != dataCenter.Id)
            {
                return BadRequest("ID yang di berikan tidak cocok dengan ID di entitas");
            }

            var existingDataCenter = await _context.DataCenters.FindAsync(id);
            if (existingDataCenter == null)
            {
                return NotFound();
            }

            existingDataCenter.Name = dataCenter.Name;
            existingDataCenter.LocationDetail = dataCenter.LocationDetail;
            existingDataCenter.ManagerName  = dataCenter.ManagerName;
            existingDataCenter.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {
                if (!DataCenterExists(id))
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

        // Metode untuk memeriksa apakah DataCenter dengan ID tertentu ada
        private bool DataCenterExists(int id)
        {
            return _context.DataCenters.Any(dc => dc.Id == id);
        }


    }
}
