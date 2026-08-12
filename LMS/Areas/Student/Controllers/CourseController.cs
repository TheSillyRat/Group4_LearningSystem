using System.Security.Claims;
using LMS.Data;
using LMS.Models;
using LMS.Models;
using LMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách các khóa học MÀ HỌC VIÊN ĐÃ ĐĂNG KÝ (My Courses)
        public async Task<IActionResult> Index()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.FullName == userName);

            if (student == null) return View(new List<Course>());

            var enrolledCourses = await _context.Enrollment
                .Where(e => e.StudentId == student.UserId)
                .Include(e => e.Course!)
                    .ThenInclude(c => c.Instructor)
                .Select(e => e.Course!)
                .ToListAsync();

            return View(enrolledCourses);
        }

        // Browse Courses: Hiển thị các thẻ Card khóa học CHƯA ĐĂNG KÝ (kèm Search & Filter theo Instructor)
        [HttpGet]
        public async Task<IActionResult> Browse(string? searchKeyword, int? instructorId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.FullName == userName);

            List<int> enrolledCourseIds = new List<int>();
            if (student != null)
            {
                enrolledCourseIds = await _context.Enrollment
                    .Where(e => e.StudentId == student.UserId)
                    .Select(e => e.CourseId)
                    .ToListAsync();
            }

            // Truy vấn các khóa học CHƯA ĐĂNG KÝ
            var query = _context.Course
                .Include(c => c.Instructor)
                .Include(c => c.Modules)
                .Where(c => !enrolledCourseIds.Contains(c.CourseId));

            // Lọc theo Từ khóa Tìm kiếm (Tên khóa học hoặc Mô tả)
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                string keyword = searchKeyword.Trim().ToLower();
                query = query.Where(c => c.CourseName.ToLower().Contains(keyword) || (c.Description != null && c.Description.ToLower().Contains(keyword)));
            }

            // Lọc theo Giảng viên
            if (instructorId.HasValue && instructorId.Value > 0)
            {
                query = query.Where(c => c.InstructorId == instructorId.Value);
            }

            var availableCourses = await query.ToListAsync();

            // Lấy danh sách các Giảng viên có khóa học đang mở để nạp vào Dropdown Filter
            var instructors = await _context.Course
                .Where(c => !enrolledCourseIds.Contains(c.CourseId) && c.Instructor != null)
                .Select(c => c.Instructor!)
                .Distinct()
                .ToListAsync();

            ViewBag.Instructors = instructors;
            ViewBag.SearchKeyword = searchKeyword;
            ViewBag.SelectedInstructorId = instructorId;

            return View(availableCourses);
        }

        // Xử lý nút Enroll Now (Đăng ký học)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.FullName == userName);

            if (student == null) return RedirectToAction("Login", "Account", new { area = "" });

            bool isEnrolled = await _context.Enrollment.AnyAsync(e => e.StudentId == student.UserId && e.CourseId == courseId);
            if (!isEnrolled)
            {
                var enrollment = new Enrollment
                {
                    StudentId = student.UserId,
                    CourseId = courseId,
                    EnrollmentDate = DateTime.Now,
                    Progress = 0,
                    Attendance = false
                };
                _context.Enrollment.Add(enrollment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Successfully enrolled in the course!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Trang xem nội dung khóa học (Learn View): Bên trái là Content, Bên phải là Mục lục Module/Lessons
        [HttpGet]
        public async Task<IActionResult> Learn(int id, int? contentId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.FullName == userName);

            if (student == null) return RedirectToAction("Login", "Account", new { area = "" });

            // Kiểm tra học viên đã đăng ký khóa học này chưa
            bool isEnrolled = await _context.Enrollment.AnyAsync(e => e.StudentId == student.UserId && e.CourseId == id);
            if (!isEnrolled)
            {
                return RedirectToAction(nameof(Browse));
            }

            // Lấy thông tin Khóa học cùng toàn bộ Modules, Contents & Quizzes
            var course = await _context.Course
                .Include(c => c.Instructor)
                .Include(c => c.Modules!)
                    .ThenInclude(m => m.Contents)
                .Include(c => c.Modules!)
                    .ThenInclude(m => m.Quizzes)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            // Lấy Bài học (Content) được chọn, nếu chưa chọn thì lấy Bài học đầu tiên của Module đầu tiên
            Content? selectedContent = null;
            if (contentId.HasValue)
            {
                selectedContent = await _context.Content.FindAsync(contentId.Value);
            }
            else
            {
                selectedContent = course.Modules?.OrderBy(m => m.DisplayOrder).FirstOrDefault()?.Contents?.OrderBy(c => c.DisplayOrder).FirstOrDefault();
            }

            ViewBag.SelectedContent = selectedContent;
            return View(course);
        }
    }
}
