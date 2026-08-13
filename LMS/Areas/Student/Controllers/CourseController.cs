using System.Security.Claims;
using LMS.Data;
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

            var enrollments = await _context.Enrollment
                .Where(e => e.StudentId == student.UserId)
                .Include(e => e.Course!)
                    .ThenInclude(c => c.Instructor)
                .ToListAsync();

            var courseProgressMap = new Dictionary<int, double>();
            foreach (var e in enrollments)
            {
                double prog = await RecalculateEnrollmentProgress(student.UserId, e.CourseId);
                courseProgressMap[e.CourseId] = prog;
            }

            ViewBag.CourseProgress = courseProgressMap;
            var enrolledCourses = enrollments.Select(e => e.Course!).ToList();

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

            // Tự động kiểm tra và bổ sung cột ModuleId, CourseId vào bảng Quizzes trong CSDL nếu thiếu
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Quizzes')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Quizzes]') AND name = 'ModuleId')
                        BEGIN
                            ALTER TABLE [Quizzes] ADD [ModuleId] int NULL;
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Quizzes]') AND name = 'CourseId')
                        BEGIN
                            ALTER TABLE [Quizzes] ADD [CourseId] int NULL;
                        END
                    END
                ");
            }
            catch { }

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

            // Đảm bảo bảng UserContentCompletion tồn tại trong SQL Server
            await _context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserContentCompletion')
                BEGIN
                    CREATE TABLE [UserContentCompletion] (
                        [UserContentCompletionId] int NOT NULL IDENTITY,
                        [StudentId] int NOT NULL,
                        [ContentId] int NOT NULL,
                        [CompletedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_UserContentCompletion] PRIMARY KEY ([UserContentCompletionId]),
                        CONSTRAINT [FK_UserContentCompletion_Users_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_UserContentCompletion_Content_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [Content] ([ContentId]) ON DELETE CASCADE
                    );
                END
            ");

            // Lấy danh sách ID các bài học đã xem xong
            var completedContentIds = await _context.UserContentCompletions
                .Where(uc => uc.StudentId == student.UserId)
                .Select(uc => uc.ContentId)
                .ToListAsync();

            // Lấy danh sách ID các bài quiz đã làm
            var completedQuizIds = await _context.QuizResults
                .Where(qr => qr.StudentId == student.UserId)
                .Select(qr => qr.QuizId)
                .Distinct()
                .ToListAsync();

            // Tính toán Tiến độ % hiện tại
            double currentProgress = await RecalculateEnrollmentProgress(student.UserId, id);

            ViewBag.Progress = currentProgress;
            ViewBag.CompletedContentIds = new HashSet<int>(completedContentIds);
            ViewBag.CompletedQuizIds = new HashSet<int>(completedQuizIds);
            ViewBag.IsSelectedContentCompleted = selectedContent != null && completedContentIds.Contains(selectedContent.ContentId);

            return View(course);
        }

        // AJAX Action: Đánh dấu Hoàn thành / Bỏ hoàn thành bài học
        [HttpPost]
        public async Task<IActionResult> ToggleContentCompletion([FromBody] ToggleCompletionRequest request)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.FullName == userName);

            if (student == null)
            {
                return Json(new { success = false, message = "Student not found" });
            }

            var content = await _context.Content
                .Include(c => c.Module)
                .FirstOrDefaultAsync(c => c.ContentId == request.ContentId);

            if (content == null || content.Module == null)
            {
                return Json(new { success = false, message = "Content not found" });
            }

            int courseId = content.Module.CourseId;

            await _context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserContentCompletion')
                BEGIN
                    CREATE TABLE [UserContentCompletion] (
                        [UserContentCompletionId] int NOT NULL IDENTITY,
                        [StudentId] int NOT NULL,
                        [ContentId] int NOT NULL,
                        [CompletedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_UserContentCompletion] PRIMARY KEY ([UserContentCompletionId]),
                        CONSTRAINT [FK_UserContentCompletion_Users_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_UserContentCompletion_Content_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [Content] ([ContentId]) ON DELETE CASCADE
                    );
                END
            ");

            var existing = await _context.UserContentCompletions
                .FirstOrDefaultAsync(uc => uc.StudentId == student.UserId && uc.ContentId == request.ContentId);

            bool isCompleted = false;
            if (existing != null)
            {
                if (request.Toggle == true)
                {
                    _context.UserContentCompletions.Remove(existing);
                    isCompleted = false;
                }
                else
                {
                    isCompleted = true;
                }
            }
            else
            {
                _context.UserContentCompletions.Add(new UserContentCompletion
                {
                    StudentId = student.UserId,
                    ContentId = request.ContentId,
                    CompletedAt = DateTime.Now
                });
                isCompleted = true;
            }

            await _context.SaveChangesAsync();

            double newProgress = await RecalculateEnrollmentProgress(student.UserId, courseId);
            return Json(new { success = true, isCompleted, progress = newProgress });
        }

        private async Task<double> RecalculateEnrollmentProgress(int studentId, int courseId)
        {
            var course = await _context.Course
                .Include(c => c.Modules!)
                    .ThenInclude(m => m.Contents)
                .Include(c => c.Modules!)
                    .ThenInclude(m => m.Quizzes)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null) return 0;

            var allContents = course.Modules?.SelectMany(m => m.Contents ?? new List<Content>()).ToList() ?? new List<Content>();
            var allQuizzes = new List<Quiz>();
            if (course.Modules != null)
            {
                foreach (var m in course.Modules)
                {
                    if (m.Quizzes != null) allQuizzes.AddRange(m.Quizzes);
                }
            }
            if (course.Quizzes != null)
            {
                foreach (var q in course.Quizzes)
                {
                    if (!allQuizzes.Any(existing => existing.QuizId == q.QuizId))
                    {
                        allQuizzes.Add(q);
                    }
                }
            }

            int totalItems = allContents.Count + allQuizzes.Count;
            if (totalItems == 0) return 0;

            var contentIds = allContents.Select(c => c.ContentId).ToList();
            var quizIds = allQuizzes.Select(q => q.QuizId).ToList();

            int completedContents = await _context.UserContentCompletions
                .Where(uc => uc.StudentId == studentId && contentIds.Contains(uc.ContentId))
                .CountAsync();

            int completedQuizzes = await _context.QuizResults
                .Where(qr => qr.StudentId == studentId && quizIds.Contains(qr.QuizId))
                .Select(qr => qr.QuizId)
                .Distinct()
                .CountAsync();

            int totalCompleted = completedContents + completedQuizzes;
            double progress = Math.Round((double)totalCompleted / totalItems * 100, 1);

            var enrollment = await _context.Enrollment
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment != null)
            {
                enrollment.Progress = progress;
                await _context.SaveChangesAsync();
            }

            return progress;
        }
    }

    public class ToggleCompletionRequest
    {
        public int ContentId { get; set; }
        public bool? Toggle { get; set; }
    }
}
