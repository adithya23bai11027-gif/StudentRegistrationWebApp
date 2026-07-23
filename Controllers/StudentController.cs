using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRegistrationWebApp.Data;
using StudentRegistrationWebApp.Models;

namespace StudentRegistrationWebApp.Controllers;

[Authorize]
public class StudentController : Controller
{
    private readonly StudentCourseDbContext _context;

    public StudentController(StudentCourseDbContext context) => _context = context;

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Index() => View(await _context.Students.Include(student => student.Course).Include(student => student.User).OrderBy(student => student.FullName).ToListAsync());

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var student = await _context.Students.Include(item => item.Course).Include(item => item.User).FirstOrDefaultAsync(item => item.StudentId == id);
        return student is null ? NotFound() : View(student);
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Register()
    {
        if (await _context.Students.AnyAsync(student => student.UserId == GetUserId()))
        {
            TempData["ErrorMessage"] = "You are already registered for a course.";
            return RedirectToAction(nameof(Profile));
        }
        await PopulateCoursesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Register([Bind("FullName,City,CourseId")] Student student)
    {
        var userId = GetUserId();
        if (await _context.Students.AnyAsync(item => item.UserId == userId))
        {
            TempData["ErrorMessage"] = "You are already registered for a course.";
            return RedirectToAction(nameof(Profile));
        }
        student.UserId = userId;
        ModelState.Remove(nameof(Student.UserId));
        if (!await _context.Courses.AnyAsync(course => course.CourseId == student.CourseId)) ModelState.AddModelError(nameof(Student.CourseId), "Choose a valid course.");
        if (!ModelState.IsValid)
        {
            await PopulateCoursesAsync(student.CourseId);
            return View(student);
        }
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "You have registered for your course successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Profile()
    {
        var student = await GetCurrentStudentAsync();
        if (student is null)
        {
            TempData["ErrorMessage"] = "Register for a course to create your student profile.";
            return RedirectToAction(nameof(Register));
        }
        return View(student);
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Edit()
    {
        var student = await GetCurrentStudentAsync();
        if (student is null) return NotFound();
        await PopulateCoursesAsync(student.CourseId);
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Edit([Bind("StudentId,FullName,City,CourseId")] Student submittedStudent)
    {
        ModelState.Remove(nameof(Student.UserId));
        var currentStudent = await _context.Students.FirstOrDefaultAsync(student => student.UserId == GetUserId());
        if (currentStudent is null || currentStudent.StudentId != submittedStudent.StudentId) return NotFound();
        if (!await _context.Courses.AnyAsync(course => course.CourseId == submittedStudent.CourseId)) ModelState.AddModelError(nameof(Student.CourseId), "Choose a valid course.");
        if (!ModelState.IsValid)
        {
            await PopulateCoursesAsync(submittedStudent.CourseId);
            return View(submittedStudent);
        }
        currentStudent.FullName = submittedStudent.FullName;
        currentStudent.City = submittedStudent.City;
        currentStudent.CourseId = submittedStudent.CourseId;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Your profile has been updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("The signed-in user could not be identified.");

    private Task<Student?> GetCurrentStudentAsync() => _context.Students.Include(student => student.Course).Include(student => student.User).FirstOrDefaultAsync(student => student.UserId == GetUserId());

    private async Task PopulateCoursesAsync(object? selectedCourse = null)
    {
        ViewData["CourseId"] = new SelectList(await _context.Courses.OrderBy(course => course.CourseName).ToListAsync(), nameof(Course.CourseId), nameof(Course.CourseName), selectedCourse);
    }
}
