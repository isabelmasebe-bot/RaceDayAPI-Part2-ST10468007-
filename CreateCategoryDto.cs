namespace RaceDayAPI.DTOs
{
    public class CreateCategoryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
    }
}
