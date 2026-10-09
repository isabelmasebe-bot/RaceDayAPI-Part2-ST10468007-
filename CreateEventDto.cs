namespace RaceDayAPI.DTOs
{
    public class CreateEventDto
    {
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal? DistanceKm { get; set; }
        public string? RouteUrl { get; set; }
        public bool WeatherEnabled { get; set; } = false;
        public string Status { get; set; } = "Upcoming"; // Upcoming, Closed, Completed
    }
}