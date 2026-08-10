using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Enrollment
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var enrollments = await _context.Enrollment
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrollmentId)
                .ToListAsync();

            var students = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "Student")
                .ToListAsync();

            ViewBag.Students = new SelectList(students, "UserId", "FullName");
            ViewBag.Courses = new SelectList(await _context.Course.ToListAsync(), "CourseId", "CourseName");

            return View(enrollments);
        }

        // POST: /Admin/Enrollment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int studentId, int courseId)
        {
            var exists = await _context.Enrollment.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
            if (exists)
            {
                TempData["ErrorMessage"] = "Student is already enrolled in this course.";
                return RedirectToAction(nameof(Index));
            }

            var newEnrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now,
                Progress = 0,
                Attendance = true
            };

            _context.Enrollment.Add(newEnrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student enrolled successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Enrollment/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollment.Remove(enrollment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Enrollment removed successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
