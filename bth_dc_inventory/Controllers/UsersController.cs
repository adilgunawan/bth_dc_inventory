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
            try
            {
                Console.WriteLine($"🔐 Login attempt for: {dto.Email}"); // Debug log

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    Console.WriteLine($"❌ User not found: {dto.Email}");
                    return Unauthorized(new { success = false, message = "Invalid email or password." });
                }

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                {
                    Console.WriteLine($"❌ Invalid password for: {dto.Email}");
                    return Unauthorized(new { success = false, message = "Invalid email or password." });
                }

                // Generate JWT Token
                var jwtKey = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];

                if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
                {
                    Console.WriteLine("❌ JWT configuration missing");
                    return StatusCode(500, new { success = false, message = "Server configuration error" });
                }

                var token = JwtHelper.GenerateJwtToken(
                    user.Id.ToString(),
                    user.Username,
                    user.Email,
                    user.Role ?? "User",
                    jwtKey,
                    jwtIssuer,
                    jwtAudience
                );

                Console.WriteLine($"✅ Login successful for: {user.Username}");

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    token = token,
                    userName = user.Username,
                    userEmail = user.Email,
                    userRole = user.Role ?? "User",
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.Email,
                        user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login exception: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        // =====================================================
        // GET CURRENT USER: GET api/users/current - ✅ UNTUK PROFILE
        // =====================================================
        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                Console.WriteLine("🔍 Getting current user from token");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                Console.WriteLine($"Claims - ID: {userIdClaim}, Name: {userName}, Email: {userEmail}, Role: {userRole}");

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    Console.WriteLine("❌ Invalid user ID in token");
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
                    Console.WriteLine($"❌ User not found in database: {userId}");
                    return NotFound(new { success = false, message = "User not found" });
                }

                Console.WriteLine($"✅ Current user retrieved: {user.Username}");

                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = user.Id,
                        userName = user.Username,
                        userEmail = user.Email,
                        userRole = user.Role ?? "User"
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetCurrentUser exception: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // =====================================================
        // GET ALL USERS: GET api/users - ✅ UNTUK SETTINGS PAGE
        // =====================================================
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            try
            {
                Console.WriteLine("📋 Getting all users");

                var users = await _context.Users
                    .Select(u => new
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role ?? "User",
                        CreatedAt = u.CreatedAt
                    })
                    .OrderBy(u => u.Username)
                    .ToListAsync();

                Console.WriteLine($"✅ Retrieved {users.Count} users");
                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetUsers exception: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching users", error = ex.Message });
            }
        }

        // =====================================================
        // GET USER STATS: GET api/users/stats - ✅ UNTUK SETTINGS PAGE
        // =====================================================
        [HttpGet("stats")]
        [Authorize]
        public async Task<ActionResult<object>> GetUserStats()
        {
            try
            {
                Console.WriteLine("📊 Getting user stats");

                var totalUsers = await _context.Users.CountAsync();
                var adminUsers = await _context.Users.CountAsync(u => u.Role == "Admin");
                var regularUsers = totalUsers - adminUsers;

                Console.WriteLine($"✅ Stats - Total: {totalUsers}, Admin: {adminUsers}, Regular: {regularUsers}");

                return Ok(new
                {
                    TotalUsers = totalUsers,
                    AdminUsers = adminUsers,
                    RegularUsers = regularUsers
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetUserStats exception: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching user stats", error = ex.Message });
            }
        }

        // =====================================================
        // ASSIGN ADMIN ROLE: PUT api/users/assign-admin/{id}
        // =====================================================
        [HttpPut("assign-admin/{id}")]
        [Authorize]
        public async Task<ActionResult> AssignAdminRole(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (currentUserRole != "Admin")
                {
                    return Forbid("Only administrators can assign admin roles");
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (user.Role == "Admin")
                {
                    return BadRequest(new { message = "User is already an admin" });
                }

                user.Role = "Admin";
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = $"{user.Username} has been assigned as admin successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error assigning admin role", error = ex.Message });
            }
        }

        // =====================================================
        // REMOVE ADMIN ROLE: PUT api/users/remove-admin/{id}
        // =====================================================
        [HttpPut("remove-admin/{id}")]
        [Authorize]
        public async Task<ActionResult> RemoveAdminRole(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (currentUserRole != "Admin")
                {
                    return Forbid("Only administrators can remove admin roles");
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (user.Role != "Admin")
                {
                    return BadRequest(new { message = "User is not an admin" });
                }

                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(currentUserIdClaim, out int currentUserId) && user.Id == currentUserId)
                {
                    return BadRequest(new { message = "You cannot remove your own admin role" });
                }

                user.Role = "User";
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = $"Admin role removed from {user.Username} successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error removing admin role", error = ex.Message });
            }
        }

        // =====================================================
        // LOGOUT: POST api/users/logout
        // =====================================================
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { success = true, message = "Logged out successfully" });
        }

        // =====================================================
        // REGISTER: POST api/users/register
        // =====================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = hashedPassword,
                Role = "User",
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

        // =====================================================
        // GET SINGLE USER: GET api/users/{id}
        // =====================================================
        [HttpGet("{id}")]
        [Authorize]
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
    }
}