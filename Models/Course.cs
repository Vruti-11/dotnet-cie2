namespace dotnet_cie2.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public decimal Fees { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Level { get; set; } = "Beginner";
        public string Icon { get; set; } = "bi-journal-code";
    }
}
