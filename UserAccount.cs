        namespace RaceDayAPI.Models
    {
        public class UserAccount
        {
            public int UserId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            // Navigation properties
            public OrganizerProfile? OrganizerProfile { get; set; }
            public ParticipantProfile? ParticipantProfile { get; set; }
        }
    }
