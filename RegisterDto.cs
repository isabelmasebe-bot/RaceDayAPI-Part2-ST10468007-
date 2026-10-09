namespace RaceDayAPI.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"

        // For Participant
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? EmergencyContact { get; set; }

        // For Organiser
        public string? OrganisationName { get; set; }
        public string? Phone { get; set; }
    }
}