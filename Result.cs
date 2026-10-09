namespace RaceDayAPI.Models
{
    public class Result
    {
        public int ResultId { get; set; }
        public int EnrollmentId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? FinishPosition { get; set; }
        public string? PerformanceStatus { get; set; } // Finished, DNF, DNS

        // Navigation properties
        public Enrollment Enrollment { get; set; } = null!;
    }
}