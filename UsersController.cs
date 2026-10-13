using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Manage the currently logged-in user's profile.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        /// <summary>
        /// Get the profile of the currently logged-in user.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("You must be logged in.");

            var user = await _context.UserAccounts
                .Include(u => u.OrganizerProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            if (user.Role == "Organiser" && user.OrganizerProfile != null)
            {
                return Ok(new
                {
                    user.UserId,
                    user.Email,
                    user.Role,
                    user.CreatedAt,
                    Profile = new
                    {
                        user.OrganizerProfile.OrganizerId,
                        user.OrganizerProfile.OrganisationName,
                        user.OrganizerProfile.ContactPhone
                    }
                });
            }

            if (user.Role == "Participant" && user.ParticipantProfile != null)
            {
                return Ok(new
                {
                    user.UserId,
                    user.Email,
                    user.Role,
                    user.CreatedAt,
                    Profile = new
                    {
                        user.ParticipantProfile.ParticipantId,
                        user.ParticipantProfile.FirstName,
                        user.ParticipantProfile.LastName,
                        user.ParticipantProfile.DateOfBirth,
                        user.ParticipantProfile.EmergencyContact
                    }
                });
            }

            return Ok(new { user.UserId, user.Email, user.Role, user.CreatedAt });
        }

        /// <summary>
        /// Update the profile of the currently logged-in user.
        /// </summary>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("You must be logged in.");

            var user = await _context.UserAccounts
                .Include(u => u.OrganizerProfile)
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            if (user.Role == "Organiser" && user.OrganizerProfile != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.OrganisationName))
                    user.OrganizerProfile.OrganisationName = dto.OrganisationName;
                if (dto.Phone != null)
                    user.OrganizerProfile.ContactPhone = dto.Phone;
            }
            else if (user.Role == "Participant" && user.ParticipantProfile != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                    user.ParticipantProfile.FirstName = dto.FirstName;
                if (!string.IsNullOrWhiteSpace(dto.LastName))
                    user.ParticipantProfile.LastName = dto.LastName;
                if (dto.DateOfBirth.HasValue)
                    user.ParticipantProfile.DateOfBirth = dto.DateOfBirth;
                if (dto.EmergencyContact != null)
                    user.ParticipantProfile.EmergencyContact = dto.EmergencyContact;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Profile updated successfully" });
        }
    }
}