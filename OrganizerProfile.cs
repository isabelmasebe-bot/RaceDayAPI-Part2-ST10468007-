using Microsoft.Extensions.Logging;


namespace RaceDayAPI.Models
{
    public class OrganizerProfile
    {
        public int OrganizerId { get; set; }
        public int UserId { get; set; }
        public string OrganisationName { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }

        // Navigation properties
        public UserAccount User { get; set; } = null!;
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}