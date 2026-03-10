using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bth_dc_inventory.Data;
using bth_dc_inventory.Models;
using bth_dc_inventory.DTOs.Users;
using bth_dc_inventory.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        // LOGIN: POST api/users/login - ✅ SUDAH OK
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
                return Unauthorized(new { success = false, message = "Invalid email or password." });
            }

            // Generate JWT Token
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            var token = JwtHelper.GenerateJwtToken(
                user.Id.ToString(),
                user.Username,
                user.Email,
                user.Role ?? "User",
                jwtKey,
                jwtIssuer,
                jwtAudience
            );

            return Ok(new
            {
                success = true,
                message = "Login successful",
                token = token,
                userName = user.Username, // ✅ Untuk frontend
                userEmail = user.Email,   // ✅ Untuk frontend
                userRole = user.Role ?? "User", // ✅ Untuk frontend
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Role
                }
            });
        }

        // =====================================================
        // GET CURRENT USER: GET api/users/current - ✅ TAMBAHAN BARU
        // =====================================================
        [HttpGet("current")]
        [Authorize] // ✅ Require JWT token
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // ✅ Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid token" });
                }

                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.Role
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        userName = user.Username,
                        userEmail = user.Email,
                        userRole = user.Role ?? "User"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // =====================================================
        // LOGOUT: POST api/users/logout - ✅ TAMBAHAN BARU
        // =====================================================
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // ✅ Untuk JWT, logout biasanya handled di client-side
            // Tapi kita bisa return success response
            return Ok(new { success = true, message = "Logged out successfully" });
        }

        // =====================================================
        // REGISTER: POST api/users/register - ✅ SUDAH OK
        // =====================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validasi email & username
            var existingEmail = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (existingEmail)
            {
                return BadRequest(new { success = false, message = "Email sudah digunakan." });
            }

            var existingUsername = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (existingUsername)
            {
                return BadRequest(new { success = false, message = "Username sudah digunakan." });
            }

            // Hash password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Create user
            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = hashedPassword,
                Role = "User", // ✅ Default role
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var userDto = new UserReadDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                Role = newUser.Role,
                CreatedAt = newUser.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, new
            {
                success = true,
                message = "User registered successfully",
                user = userDto
            });
        }

        // ✅ REST OF YOUR EXISTING ENDPOINTS...
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

        // ✅ ... rest of your existing endpoints
    }
}