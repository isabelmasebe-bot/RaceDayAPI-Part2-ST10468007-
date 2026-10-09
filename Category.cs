namespace RaceDayAPI.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        // Navigation properties
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }
}
