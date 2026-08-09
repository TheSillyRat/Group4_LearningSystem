using LMS.Models;
using LMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Student.Controllers
{
    [Area("Student")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly int _mockStudentId = 1; // Temporary mock user ID until Auth is implemented

        public CourseController(ICourseService courseService, IEnrollmentService enrollmentService)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
        }

        // GET: Student/Course (Browse Courses)
        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return View(courses);
        }

        // GET: Student/Course/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.IsEnrolled = await _enrollmentService.IsEnrolledAsync(_mockStudentId, id);

            return View(course);
        }
    }
}
