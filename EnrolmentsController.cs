using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [Route("api/enrolments")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnrolmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
        private string? GetRole() => HttpContext.Session.GetString("Role");

        // POST: /api/enrolments
        // Participant only – enrol in an event + category
        [HttpPost]
        public async Task<IActionResult> Enrol([FromBody] EnrolDto dto)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Participant")
                return Forbid();

            // Get the participant profile
            var participant = await _context.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return BadRequest("Participant profile not found.");

            // Check if the event exists and is still open
            var eventItem = await _context.Events.FindAsync(dto.EventId);
            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.Status != "Upcoming")
                return BadRequest("This event is closed or completed. Enrolment is not allowed.");

            // Find the EventCategory link
            var eventCategory = await _context.EventCategories
                .FirstOrDefaultAsync(ec => ec.EventId == dto.EventId && ec.CategoryId == dto.CategoryId);

            if (eventCategory == null)
                return BadRequest("This category is not available for the selected event.");

            // Check if already enrolled
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ParticipantId == participant.ParticipantId
                            && e.EventCategoryId == eventCategory.EventCategoryId
                            && e.RegistrationStatus != "Cancelled");

            if (alreadyEnrolled)
                return Conflict("You are already enrolled in this event category.");

            // Create the enrolment
            var enrolment = new Enrollment
            {
                EventCategoryId = eventCategory.EventCategoryId,
                ParticipantId = participant.ParticipantId,
                RegistrationStatus = "Confirmed",
                RegisteredAt = DateTime.UtcNow,
                BibNumber = null // Can be assigned later by organiser
            };

            _context.Enrollments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEnrolments), new { id = enrolment.EnrollmentId }, new
            {
                enrolment.EnrollmentId,
                Message = "Successfully enrolled"
            });
        }

        // GET: /api/enrolments/me
        // Participant only – list my enrolments + results
        [HttpGet("me")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Participant")
                return Forbid();

            var participant = await _context.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return BadRequest("Participant profile not found.");

            var enrolments = await _context.Enrollments
                .Where(e => e.ParticipantId == participant.ParticipantId)
                .Include(e => e.EventCategory)
                    .ThenInclude(ec => ec.Event)
                .Include(e => e.EventCategory)
                    .ThenInclude(ec => ec.Category)
                .Include(e => e.Result)
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.BibNumber,
                    e.RegistrationStatus,
                    e.RegisteredAt,
                    Event = new
                    {
                        e.EventCategory.Event.EventId,
                        e.EventCategory.Event.EventName,
                        e.EventCategory.Event.EventDate,
                        e.EventCategory.Event.Location,
                        e.EventCategory.Event.Status
                    },
                    Category = new
                    {
                        e.EventCategory.Category.CategoryName,
                        e.EventCategory.EntryFee
                    },
                    Result = e.Result == null ? null : new
                    {
                        e.Result.FinishTime,
                        e.Result.FinishPosition,
                        e.Result.PerformanceStatus
                    }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // PUT: /api/enrolments/{id}/cancel
        // Participant only – cancel own enrolment
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Participant")
                return Forbid();

            var participant = await _context.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return BadRequest("Participant profile not found.");

            var enrolment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            // Ownership check
            if (enrolment.ParticipantId != participant.ParticipantId)
                return Forbid();

            if (enrolment.RegistrationStatus == "Cancelled")
                return BadRequest("This enrolment is already cancelled.");

            enrolment.RegistrationStatus = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Enrolment cancelled successfully" });
        }
    }
}
