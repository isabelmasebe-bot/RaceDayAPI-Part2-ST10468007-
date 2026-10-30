using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Participant"; // Organiser or Participant

        // Participant fields
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Organiser fields
        public string? OrganisationName { get; set; }
        public string? Phone { get; set; }
    }
}