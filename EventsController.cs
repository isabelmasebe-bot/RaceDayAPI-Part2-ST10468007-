using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Manage events, categories linked to events, enrolments and results.
    /// </summary>
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
        private string? GetRole() => HttpContext.Session.GetString("Role");

        /// <summary>
        /// Get a list of all events. Can optionally filter by status (Upcoming, Closed, Completed).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllEvents([FromQuery] string? status = null)
        {
            var query = _context.Events
                .Include(e => e.Organizer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            var events = await query
                .Select(e => new
                {
                    e.EventId,
                    e.EventName,
                    e.EventDate,
                    e.Location,
                    e.DistanceKm,
                    e.RouteUrl,
                    e.WeatherEnabled,
                    e.Status,
                    Organiser = e.Organizer.OrganisationName
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Get detailed information about a single event including its categories.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventById(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.EventCategories)
                    .ThenInclude(ec => ec.Category)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            return Ok(new
            {
                eventItem.EventId,
                eventItem.EventName,
                eventItem.EventDate,
                eventItem.Location,
                eventItem.DistanceKm,
                eventItem.RouteUrl,
                eventItem.WeatherEnabled,
                eventItem.Status,
                Organiser = eventItem.Organizer.OrganisationName,
                Categories = eventItem.EventCategories.Select(ec => new
                {
                    ec.EventCategoryId,
                    CategoryName = ec.Category.CategoryName,
                    ec.Category.MinAge,
                    ec.Category.MaxAge,
                    ec.EntryFee
                })
            });
        }

        /// <summary>
        /// Create a new event. Only Organisers can perform this action.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return StatusCode(403, "Only Organisers can create events.");

            var organizer = await _context.OrganizerProfiles
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organizer == null)
                return BadRequest("Organiser profile not found.");

            var newEvent = new Event
            {
                OrganizerId = organizer.OrganizerId,
                EventName = dto.EventName,
                EventDate = dto.EventDate,
                Location = dto.Location,
                DistanceKm = dto.DistanceKm,
                RouteUrl = dto.RouteUrl,
                WeatherEnabled = dto.WeatherEnabled,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Upcoming" : dto.Status
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventId }, new
            {
                newEvent.EventId,
                newEvent.EventName,
                Message = "Event created successfully"
            });
        }

        /// <summary>
        /// Update an existing event. Only the Organiser who owns the event can update it.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return StatusCode(403, "Only Organisers can update events.");

            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.Organizer.UserId != userId)
                return StatusCode(403, "You can only update your own events.");

            if (!string.IsNullOrWhiteSpace(dto.EventName))
                eventItem.EventName = dto.EventName;
            if (dto.EventDate.HasValue)
                eventItem.EventDate = dto.EventDate.Value;
            if (!string.IsNullOrWhiteSpace(dto.Location))
                eventItem.Location = dto.Location;
            if (dto.DistanceKm.HasValue)
                eventItem.DistanceKm = dto.DistanceKm;
            if (dto.RouteUrl != null)
                eventItem.RouteUrl = dto.RouteUrl;
            if (dto.WeatherEnabled.HasValue)
                eventItem.WeatherEnabled = dto.WeatherEnabled.Value;
            if (!string.IsNullOrWhiteSpace(dto.Status))
                eventItem.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Event updated successfully" });
        }

        /// <summary>
        /// Soft-delete (close) an event. Only the owning Organiser can perform this action.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return StatusCode(403, "Only Organisers can delete events.");

            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.Organizer.UserId != userId)
                return StatusCode(403, "You can only delete your own events.");

            eventItem.Status = "Closed";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get all categories linked to a specific event.
        /// </summary>
        [HttpGet("{eventId}/categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventCategories(int eventId)
        {
            var exists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!exists)
                return NotFound("Event not found.");

            var categories = await _context.EventCategories
                .Where(ec => ec.EventId == eventId)
                .Include(ec => ec.Category)
                .Select(ec => new
                {
                    ec.EventCategoryId,
                    ec.CategoryId,
                    CategoryName = ec.Category.CategoryName,
                    ec.Category.MinAge,
                    ec.Category.MaxAge,
                    ec.EntryFee
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Add a category to an event (with entry fee). Only the owning Organiser can do this.
        /// </summary>
        [HttpPost("{eventId}/categories")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddCategoryToEvent(int eventId, [FromBody] AddCategoryToEventDto dto)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return StatusCode(403, "Only Organisers can add categories to events.");

            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.Organizer.UserId != userId)
                return StatusCode(403, "You can only manage categories for your own events.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);
            if (!categoryExists)
                return BadRequest("Category does not exist.");

            var alreadyLinked = await _context.EventCategories
                .AnyAsync(ec => ec.EventId == eventId && ec.CategoryId == dto.CategoryId);

            if (alreadyLinked)
                return Conflict("This category is already linked to the event.");

            var eventCategory = new EventCategory
            {
                EventId = eventId,
                CategoryId = dto.CategoryId,
                EntryFee = dto.EntryFee
            };

            _context.EventCategories.Add(eventCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventCategories), new { eventId }, new
            {
                eventCategory.EventCategoryId,
                Message = "Category added to event successfully"
            });
        }

        /// <summary>
        /// Get all enrolments for a specific event. Only the owning Organiser can view this.
        /// </summary>
        [HttpGet("{eventId}/enrolments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return StatusCode(403, "Only Organisers can view enrolments.");

            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.Organizer.UserId != userId)
                return StatusCode(403, "You can only view enrolments for your own events.");

            var enrolments = await _context.Enrollments
                .Where(e => e.EventCategory.EventId == eventId)
                .Include(e => e.Participant)
                .Include(e => e.EventCategory)
                    .ThenInclude(ec => ec.Category)
                .Include(e => e.Result)
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.BibNumber,
                    e.RegistrationStatus,
                    e.RegisteredAt,
                    Participant = new
                    {
                        e.Participant.FirstName,
                        e.Participant.LastName,
                        e.Participant.ParticipantId
                    },
                    Category = e.EventCategory.Category.CategoryName,
                    EntryFee = e.EventCategory.EntryFee,
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

        /// <summary>
        /// Get the results / leaderboard for a specific event (ordered by position).
        /// </summary>
        [HttpGet("{eventId}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var exists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!exists)
                return NotFound("Event not found.");

            var results = await _context.Results
                .Where(r => r.Enrollment.EventCategory.EventId == eventId)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.EventCategory)
                        .ThenInclude(ec => ec.Category)
                .OrderBy(r => r.FinishPosition ?? int.MaxValue)
                .Select(r => new
                {
                    r.ResultId,
                    r.FinishTime,
                    r.FinishPosition,
                    r.PerformanceStatus,
                    Participant = new
                    {
                        r.Enrollment.Participant.FirstName,
                        r.Enrollment.Participant.LastName,
                        r.Enrollment.BibNumber
                    },
                    Category = r.Enrollment.EventCategory.Category.CategoryName
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}