using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using BCrypt.Net;

namespace RaceDayAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and Password are required.");

            if (dto.Role != "Organiser" && dto.Role != "Participant")
                return BadRequest("Role must be either 'Organiser' or 'Participant'.");

            // Check if email already exists
            if (await _context.UserAccounts.AnyAsync(u => u.Email == dto.Email))
                return Conflict("Email already exists.");

            // Create UserAccount
            var user = new UserAccount
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();

            // Create profile based on role
            if (dto.Role == "Organiser")
            {
                var organizer = new OrganizerProfile
                {
                    UserId = user.UserId,
                    OrganisationName = dto.OrganisationName ?? "My Organisation",
                    ContactPhone = dto.Phone
                };
                _context.OrganizerProfiles.Add(organizer);
            }
            else // Participant
            {
                var participant = new ParticipantProfile
                {
                    UserId = user.UserId,
                    FirstName = dto.FirstName ?? "",
                    LastName = dto.LastName ?? "",
                    DateOfBirth = dto.DateOfBirth,
                    EmergencyContact = dto.EmergencyContact
                };
                _context.ParticipantProfiles.Add(participant);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = user.UserId }, new
            {
                user.UserId,
                user.Email,
                user.Role,
                Message = "Registration successful"
            });
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and Password are required.");

            var user = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            // Store user info in Session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Email", user.Email);

            return Ok(new
            {
                Message = "Login successful",
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role
            });
        }

        // POST: /api/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { Message = "Logged out successfully" });
        }

        // GET: /api/auth/me  (to check who is currently logged in)
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");
            var email = HttpContext.Session.GetString("Email");

            if (userId == null)
                return Unauthorized("Not logged in.");

            return Ok(new
            {
                UserId = userId,
                Email = email,
                Role = role
            });
        }
    }
}