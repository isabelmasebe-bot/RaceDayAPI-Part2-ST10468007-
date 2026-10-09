namespace RaceDayAPI.Models
{
    public class ParticipantProfile
    {
        public int ParticipantId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? EmergencyContact { get; set; }

        // Navigation properties
        public UserAccount User { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}