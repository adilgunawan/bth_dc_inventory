using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.Models;
using bth_dc_inventory.DTOs.Users;
using bth_dc_inventory.Helpers;

namespace bth_dc_inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, IConfiguration configuration)
        {
                    this._context = context;
                    _configuration = configuration;
        }

        // =====================================================
        // GET: api/users
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserReadDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // =====================================================
        // GET: api/users/{id}
        // =====================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserReadDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // =====================================================
        // POST: api/users
        // =====================================================
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> PostUser(UserCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password, // ⚠️ akan di-hash nanti
                Role = "user",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, result);
        }

        // =====================================================
        // PUT: api/users/{id}
        // =====================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            existingUser.Username = dto.Username;
            existingUser.Email = dto.Email;
            existingUser.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =====================================================
        // DELETE: api/users/{id}
        // =====================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // =====================================================
        // REGISTER: POST api/users/register
        // =====================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserCreateDto dto)
        {
            // 1. Validasi apakah `Email` atau `Username` sudah ada di database.
            var existingEmail = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (existingEmail)
            {
                return BadRequest(new { message = "Email sudah digunakan." });
            }

            var existingUsername = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (existingUsername)
            {
                return BadRequest(new { message = "Username sudah digunakan." });
            }

            // 2. Hash password menggunakan BCrypt.
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Buat user baru.
            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = hashedPassword, // Simpan password yang sudah di-hash
                CreatedAt = DateTime.Now
            };

            // 4. Simpan user ke database.
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // 5. Konversi ke DTO untuk dikembalikan ke client.
            var userDto = new UserReadDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                Role = newUser.Role,
                CreatedAt = newUser.CreatedAt
            };

            // 6. Kembalikan response `201 Created`.
            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, userDto);
        }

        // =====================================================
        // LOGIN: POST api/users/login
        // =====================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Ambil config JWT dari appsettings.json
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            var token = JwtHelper.GenerateJwtToken(
                user.Id.ToString(),
                user.Username,
                user.Email,
                user.Role,
                jwtKey,
                jwtIssuer,
                jwtAudience
            );

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Role
                }
            });
        }
    }
}


//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using bth_dc_inventory.Data;
//using bth_dc_inventory.Models;


//namespace bth_dc_inventory.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UsersController : ControllerBase    
//    {
//        private readonly ApplicationDbContext _context;

//        public UsersController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        //GET: api/users
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
//        {
//            return await _context.Users.ToListAsync();
//        }

//        //GET: api/user/5
//        [HttpGet("{id}")]
//        public async Task<ActionResult<User>> GetUser(int id)
//        {
//            var user = await _context.Users.FindAsync(id);

//            if (user == null)
//            {
//                return NotFound(); // Jika user tidak ditemukan, kembalikan status HTTP 404
//            }

//            return Ok(user); // Jika user ditemukan, kembalikan data user dengan HTTP 200 OK
//        }

//        //POST: api/Users
//        [HttpPost]
//        public async Task<ActionResult<User>> PostUser(User user)
//        {
//            _context.Users.Add(user);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction("GetUser", new { id = user.Id }, user);
//        }

//        //PUT: api/user/5
//        [HttpPut("{id}")]
//        public async Task<IActionResult> PutUser(int id, User user)
//        {
//            if (id != user.Id)
//            {
//                return BadRequest("Id mismatch between route and body.");
//            }

//            // Temukan user dari database berdasarkan id
//            var existingUser = await _context.Users.FindAsync(id);
//            if (existingUser == null)
//            {
//                return NotFound();
//            }

//            // Perbarui hanya properti yang diubah
//            existingUser.Username = user.Username;
//            existingUser.Email = user.Email;
//            existingUser.Password = user.Password; // Harusnya dienkripsi
//            existingUser.Role = user.Role;

//            try
//            {
//                await _context.SaveChangesAsync();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!UserExists(id))
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

//        // Metode pengecekan apakah User dengan ID tertentu ada
//        private bool UserExists(int id)
//        {
//            return _context.Users.Any(e => e.Id == id);
//        }
//    }
//}
