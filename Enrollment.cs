
namespace RaceDayAPI.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int EventCategoryId { get; set; }
        public int ParticipantId { get; set; }
        public string? BibNumber { get; set; }
        public string RegistrationStatus { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public EventCategory EventCategory { get; set; } = null!;
        public ParticipantProfile Participant { get; set; } = null!;
        public Result? Result { get; set; }
    }
}