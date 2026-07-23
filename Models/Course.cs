using System.ComponentModel.DataAnnotations;

namespace StudentRegistrationWebApp.Models;

public class Course
{
    public int CourseId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Course Name")]
    public string CourseName { get; set; } = string.Empty;

    [Range(1, 120)]
    [Display(Name = "Duration (Months)")]
    public int CourseDuration { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
