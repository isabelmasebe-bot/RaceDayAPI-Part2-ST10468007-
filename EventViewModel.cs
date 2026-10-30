using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(14);

        [Required]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Distance (km)")]
        public decimal? DistanceKm { get; set; }

        [Display(Name = "Route URL")]
        public string? RouteUrl { get; set; }

        [Display(Name = "Enable Weather")]
        public bool WeatherEnabled { get; set; }

        public string Status { get; set; } = "Upcoming";

        public string? BannerUrl { get; set; }

        // For file upload
        [Display(Name = "Event Banner")]
        public IFormFile? BannerFile { get; set; }
    }
}