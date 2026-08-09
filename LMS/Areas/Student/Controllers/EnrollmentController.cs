using LMS.Models;
using LMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Student.Controllers
{
    [Area("Student")]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly int _mockStudentId = 1; // Temporary mock user ID until Auth is implemented

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        // GET: Student/Enrollment/MyCourses
        public async Task<IActionResult> MyCourses()
        {
            var enrollments = await _enrollmentService.GetMyCoursesAsync(_mockStudentId);
            return View(enrollments);
        }

        // POST: Student/Enrollment/Enroll/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int id)
        {
            var result = await _enrollmentService.EnrollStudentAsync(_mockStudentId, id);
            if (result)
            {
                TempData["Message"] = "Successfully enrolled in the course.";
            }
            else
            {
                TempData["Message"] = "You are already enrolled in this course.";
            }
            return RedirectToAction("Details", "Course", new { area = "Student", id = id });
        }

        // POST: Student/Enrollment/CancelEnrollment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrollment(int id)
        {
            var result = await _enrollmentService.CancelEnrollmentAsync(_mockStudentId, id);
            if (result)
            {
                TempData["Message"] = "Enrollment cancelled successfully.";
            }
            return RedirectToAction("Details", "Course", new { area = "Student", id = id });
        }
    }
}
