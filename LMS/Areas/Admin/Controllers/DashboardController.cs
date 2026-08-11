using LMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalAdmins = await _context.Users.Include(u => u.Role).CountAsync(u => u.Role != null && u.Role.RoleName == "Admin");
            var totalInstructors = await _context.Users.Include(u => u.Role).CountAsync(u => u.Role != null && u.Role.RoleName == "Instructor");
            var totalStudents = await _context.Users.Include(u => u.Role).CountAsync(u => u.Role != null && u.Role.RoleName == "Student");

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalAdmins = totalAdmins;
            ViewBag.TotalInstructors = totalInstructors;
            ViewBag.TotalStudents = totalStudents;

            ViewBag.TotalCourses = await _context.Course.CountAsync();
            ViewBag.TotalEnrollments = await _context.Enrollment.CountAsync();
            ViewBag.TotalAssignments = await _context.Assignments.CountAsync();

            var recentUsers = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.UserId)
                .Take(6)
                .ToListAsync();

            var recentCourses = await _context.Course
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .OrderByDescending(c => c.CourseId)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentCourses = recentCourses;

            return View(recentUsers);
        }
    }
}
