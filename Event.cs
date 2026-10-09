namespace RaceDayAPI.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public int OrganizerId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal? DistanceKm { get; set; }
        public string? RouteUrl { get; set; }
        public bool WeatherEnabled { get; set; }
        public string Status { get; set; } = "Upcoming"; // Upcoming, Closed, Completed

        // Navigation properties
        public OrganizerProfile Organizer { get; set; } = null!;
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }
}