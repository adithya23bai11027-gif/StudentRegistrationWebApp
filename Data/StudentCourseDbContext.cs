using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentRegistrationWebApp.Models;

namespace StudentRegistrationWebApp.Data;

public class StudentCourseDbContext : IdentityDbContext
{
    public StudentCourseDbContext(DbContextOptions<StudentCourseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>().HasData(
            new Course { CourseId = 1, CourseName = "ASP.NET Core MVC", CourseDuration = 6 },
            new Course { CourseId = 2, CourseName = "Machine Learning", CourseDuration = 8 });

        builder.Entity<Student>()
            .HasOne(student => student.Course)
            .WithMany(course => course.Students)
            .HasForeignKey(student => student.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Student>()
            .HasOne(student => student.User)
            .WithMany()
            .HasForeignKey(student => student.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Student>()
            .HasIndex(student => student.UserId)
            .IsUnique();
    }
}
