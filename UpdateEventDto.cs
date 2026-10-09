namespace RaceDayAPI.DTOs
{
    public class UpdateEventDto
    {
        public string? EventName { get; set; }
        public DateTime? EventDate { get; set; }
        public string? Location { get; set; }
        public decimal? DistanceKm { get; set; }
        public string? RouteUrl { get; set; }
        public bool? WeatherEnabled { get; set; }
        public string? Status { get; set; }
    }
}