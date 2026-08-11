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
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Course
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Course
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .OrderByDescending(c => c.CourseId)
                .ToListAsync();

            return View(courses);
        }

        // GET: /Admin/Course/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Course
                .Include(c => c.Instructor)
                .Include(c => c.Modules!)
                .Include(c => c.Enrollments!)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: /Admin/Course/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var instructors = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "Instructor")
                .ToListAsync();

            ViewBag.Instructors = new SelectList(instructors, "UserId", "FullName");
            return View();
        }

        // POST: /Admin/Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (!ModelState.IsValid)
            {
                var instructors = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role != null && u.Role.RoleName == "Instructor")
                    .ToListAsync();
                ViewBag.Instructors = new SelectList(instructors, "UserId", "FullName", course.InstructorId);
                return View(course);
            }

            _context.Course.Add(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Course created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Course/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var instructors = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "Instructor")
                .ToListAsync();

            ViewBag.Instructors = new SelectList(instructors, "UserId", "FullName", course.InstructorId);
            return View(course);
        }

        // POST: /Admin/Course/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.CourseId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var instructors = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role != null && u.Role.RoleName == "Instructor")
                    .ToListAsync();
                ViewBag.Instructors = new SelectList(instructors, "UserId", "FullName", course.InstructorId);
                return View(course);
            }

            _context.Update(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Course updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Course/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course != null)
            {
                _context.Course.Remove(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Course deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
