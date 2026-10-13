using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using BCrypt.Net;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Handles user registration, login, logout and current session information.
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Register a new user as either Organiser or Participant.
        /// </summary>
        /// <param name="dto">Registration details including role</param>
        /// <returns>201 Created with user info, 400 validation error, or 409 if email already exists</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and Password are required.");

            if (dto.Role != "Organiser" && dto.Role != "Participant")
                return BadRequest("Role must be either 'Organiser' or 'Participant'.");

            if (await _context.UserAccounts.AnyAsync(u => u.Email == dto.Email))
                return Conflict("Email already exists.");

            var user = new UserAccount
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();

            if (dto.Role == "Organiser")
            {
                _context.OrganizerProfiles.Add(new OrganizerProfile
                {
                    UserId = user.UserId,
                    OrganisationName = dto.OrganisationName ?? "My Organisation",
                    ContactPhone = dto.Phone
                });
            }
            else
            {
                _context.ParticipantProfiles.Add(new ParticipantProfile
                {
                    UserId = user.UserId,
                    FirstName = dto.FirstName ?? "",
                    LastName = dto.LastName ?? "",
                    DateOfBirth = dto.DateOfBirth,
                    EmergencyContact = dto.EmergencyContact
                });
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

        /// <summary>
        /// Authenticate a user and create a session.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and Password are required.");

            var user = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

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

        /// <summary>
        /// Clear the current user session (logout).
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { Message = "Logged out successfully" });
        }

        /// <summary>
        /// Get information about the currently logged-in user.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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