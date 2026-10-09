using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper methods
        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
        private string? GetRole() => HttpContext.Session.GetString("Role");

        // GET: /api/events
        // Public – list all events (optional filter by status)
        [HttpGet]
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

        // GET: /api/events/{id}
        // Public – get single event + categories
        [HttpGet("{id}")]
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

        // POST: /api/events
        // Organiser only
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return Forbid(); // 403

            // Find the OrganizerProfile linked to this user
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
                Status = dto.Status
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

        // PUT: /api/events/{id}
        // Only the owner Organiser can update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return Forbid();

            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            // Check ownership
            if (eventItem.Organizer.UserId != userId)
                return Forbid(); // Not the owner

            // Update only the fields that were sent
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

        // DELETE: /api/events/{id}
        // Soft delete = set status to "Closed" (or hard delete if you prefer)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (userId == null)
                return Unauthorized("You must be logged in.");

            if (role != "Organiser")
                return Forbid();

            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound("Event not found.");

            // Ownership check
            if (eventItem.Organizer.UserId != userId)
                return Forbid();

            // Soft delete
            eventItem.Status = "Closed";
            await _context.SaveChangesAsync();

            // If you want hard delete instead, use:
            // _context.Events.Remove(eventItem);
            // await _context.SaveChangesAsync();

            return NoContent(); // 204
        }
    }
}
