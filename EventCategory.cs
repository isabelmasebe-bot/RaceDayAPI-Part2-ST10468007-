namespace RaceDayAPI.Models
{
    public class EventCategory
    {
        public int EventCategoryId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public decimal EntryFee { get; set; }

        // Navigation properties
        public Event Event { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
