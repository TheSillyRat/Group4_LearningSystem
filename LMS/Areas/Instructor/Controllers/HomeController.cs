using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = new InstructorDashboardViewModel
            {
                RecentAssignments = await _context.Assignments
                    .OrderBy(a => a.DueDate)
                    .Take(5)
                    .ToListAsync(),

                MyCourses = await _context.Course
                    .OrderByDescending(c => c.CourseId)
                    .Take(4)
                    .ToListAsync()
            };

            return View(dashboardData);
        }
    }
}