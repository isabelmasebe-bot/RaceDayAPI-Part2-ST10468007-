using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using System.Globalization;

namespace RaceDayAPI.Controllers
{
    [Route("api/results")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
        private string? GetRole() => HttpContext.Session.GetString("Role");

        // POST: /api/results
        [HttpPost]
        public async Task<IActionResult> CreateResult([FromBody] CreateResultDto dto)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return StatusCode(403, "Only Organisers can record results.");

            var enrolment = await _context.Enrollments
                .Include(e => e.EventCategory)
                    .ThenInclude(ec => ec.Event)
                        .ThenInclude(ev => ev.Organizer)
                .FirstOrDefaultAsync(e => e.EnrollmentId == dto.EnrollmentId);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.EventCategory.Event.Organizer.UserId != userId)
                return StatusCode(403, "You can only record results for your own events.");

            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrollmentId == dto.EnrollmentId);

            if (existingResult != null)
                return Conflict("A result already exists for this enrolment.");

            TimeSpan? finishTime = null;
            if (!string.IsNullOrWhiteSpace(dto.FinishTime))
            {
                if (TimeSpan.TryParse(dto.FinishTime, CultureInfo.InvariantCulture, out var parsed))
                    finishTime = parsed;
                else
                    return BadRequest("FinishTime must be in format HH:mm:ss (e.g. 01:23:45)");
            }

            var result = new Result
            {
                EnrollmentId = dto.EnrollmentId,
                FinishTime = finishTime,
                FinishPosition = dto.FinishPosition,
                PerformanceStatus = dto.PerformanceStatus
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResultById), new { id = result.ResultId }, new
            {
                result.ResultId,
                Message = "Result recorded successfully"
            });
        }

        // GET: /api/results/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResultById(int id)
        {
            var result = await _context.Results
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.EventCategory)
                        .ThenInclude(ec => ec.Event)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.EventCategory)
                        .ThenInclude(ec => ec.Category)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound("Result not found.");

            return Ok(new
            {
                result.ResultId,
                result.FinishTime,
                result.FinishPosition,
                result.PerformanceStatus,
                Participant = new
                {
                    result.Enrollment.Participant.FirstName,
                    result.Enrollment.Participant.LastName
                },
                Event = result.Enrollment.EventCategory.Event.EventName,
                Category = result.Enrollment.EventCategory.Category.CategoryName
            });
        }

        // GET: /api/users/me/results
        [HttpGet("/api/users/me/results")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Participant")
                return StatusCode(403, "Only Participants can view their results.");

            var participant = await _context.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return BadRequest("Participant profile not found.");

            var results = await _context.Results
                .Where(r => r.Enrollment.ParticipantId == participant.ParticipantId)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.EventCategory)
                        .ThenInclude(ec => ec.Event)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.EventCategory)
                        .ThenInclude(ec => ec.Category)
                .Select(r => new
                {
                    r.ResultId,
                    r.FinishTime,
                    r.FinishPosition,
                    r.PerformanceStatus,
                    Event = r.Enrollment.EventCategory.Event.EventName,
                    EventDate = r.Enrollment.EventCategory.Event.EventDate,
                    Category = r.Enrollment.EventCategory.Category.CategoryName
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}