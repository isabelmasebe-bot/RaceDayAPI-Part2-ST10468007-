namespace RaceDayAPI.DTOs
{
    public class UpdateProfileDto
    {
        // Common
        public string? Phone { get; set; }

        // For Participant
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? EmergencyContact { get; set; }

        // For Organiser
        public string? OrganisationName { get; set; }
    }
}