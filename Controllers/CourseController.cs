using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentRegistrationWebApp.Data;
using StudentRegistrationWebApp.Models;

namespace StudentRegistrationWebApp.Controllers;

public class CourseController : Controller
{
    private readonly StudentCourseDbContext _context;

    public CourseController(StudentCourseDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Courses.OrderBy(course => course.CourseName).ToListAsync());
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var course = await _context.Courses.Include(item => item.Students).FirstOrDefaultAsync(item => item.CourseId == id);
        return course is null ? NotFound() : View(course);
    }

    [Authorize(Roles = "Administrator")]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create([Bind("CourseName,CourseDuration")] Course course)
    {
        if (!ModelState.IsValid) return View(course);
        _context.Add(course);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Course created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var course = await _context.Courses.FindAsync(id);
        return course is null ? NotFound() : View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int id, [Bind("CourseId,CourseName,CourseDuration")] Course course)
    {
        if (id != course.CourseId) return NotFound();
        if (!ModelState.IsValid) return View(course);
        try
        {
            _context.Update(course);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Course updated successfully.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Courses.AnyAsync(item => item.CourseId == course.CourseId)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var course = await _context.Courses.FirstOrDefaultAsync(item => item.CourseId == id);
        return course is null ? NotFound() : View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course is null) return RedirectToAction(nameof(Index));
        if (await _context.Students.AnyAsync(student => student.CourseId == id))
        {
            TempData["ErrorMessage"] = "A course with registered students cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Course deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
