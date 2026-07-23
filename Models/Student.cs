using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudentRegistrationWebApp.Models;

public class Student
{
    public int StudentId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public Course? Course { get; set; }

    public IdentityUser? User { get; set; }
}
