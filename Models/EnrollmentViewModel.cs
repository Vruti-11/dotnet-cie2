using System.ComponentModel.DataAnnotations;

namespace dotnet_cie2.Models
{
    public class EnrollmentViewModel
    {
        [Required]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Student name is required.")]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email Address")]
        public string StudentEmail { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        public Course? CourseDetails { get; set; }
    }
}
