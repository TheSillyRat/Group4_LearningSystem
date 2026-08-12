using System;

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Student.Controllers
{
    [Area("Student")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang bắt đầu làm bài trắc nghiệm (Take Quiz)
        [HttpGet]
        public async Task<IActionResult> Take(int id)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.FullName == userName);

            if (student == null) return RedirectToAction("Login", "Account", new { area = "" });

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Module)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null) return NotFound();

            // Kiểm tra học viên đã đăng ký khóa học này chưa
            bool isEnrolled = await _context.Enrollment.AnyAsync(e => e.StudentId == student.UserId && e.CourseId == quiz.CourseId);
            if (!isEnrolled)
            {
                return RedirectToAction("Browse", "Course");
            }

            return View(quiz);
        }

        // Xử lý Nộp bài làm trắc nghiệm (Submit Quiz)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int quizId, IFormCollection form)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.FullName == userName);

            if (student == null) return RedirectToAction("Login", "Account", new { area = "" });

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null) return NotFound();

            int totalQuestions = quiz.Questions?.Count ?? 0;
            int correctCount = 0;
            Dictionary<int, string> studentAnswers = new Dictionary<int, string>();

            if (quiz.Questions != null && totalQuestions > 0)
            {
                foreach (var q in quiz.Questions)
                {
                    string inputKey = "q_" + q.QuestionId;
                    var selectedValues = form[inputKey];
                    string studentAnswer = string.Join(", ", selectedValues.Select(v => v?.Trim())).Trim();
                    studentAnswers[q.QuestionId] = studentAnswer;

                    if (q.QuestionType == "SingleChoice")
                    {
                        if (!string.IsNullOrEmpty(studentAnswer) && !string.IsNullOrEmpty(q.CorrectAnswer) &&
                            studentAnswer.Equals(q.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            correctCount++;
                        }
                    }
                    else if (q.QuestionType == "MultipleChoice")
                    {
                        // So sánh các đáp án MultipleChoice (ví dụ "A, C")
                        if (!string.IsNullOrEmpty(studentAnswer) && !string.IsNullOrEmpty(q.CorrectAnswer))
                        {
                            var expectedSet = q.CorrectAnswer.Split(',').Select(x => x.Trim().ToUpper()).OrderBy(x => x).ToList();
                            var actualSet = studentAnswer.Split(',').Select(x => x.Trim().ToUpper()).OrderBy(x => x).ToList();

                            if (expectedSet.SequenceEqual(actualSet))
                            {
                                correctCount++;
                            }
                        }
                    }
                }
            }

            double score = totalQuestions > 0 ? Math.Round((double)correctCount / totalQuestions * 100, 1) : 0;

            var quizResult = new QuizResult
            {
                QuizId = quizId,
                StudentId = student.UserId,
                Score = score,
                CorrectAnswers = correctCount,
                TotalQuestions = totalQuestions,
                SubmittedAt = DateTime.Now
            };

            // Tự động kiểm tra và tạo bảng QuizResults trong SQL Server nếu chưa có
            await _context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuizResults')
                BEGIN
                    CREATE TABLE [QuizResults] (
                        [QuizResultId] int NOT NULL IDENTITY,
                        [QuizId] int NOT NULL,
                        [StudentId] int NOT NULL,
                        [Score] float NOT NULL,
                        [CorrectAnswers] int NOT NULL,
                        [TotalQuestions] int NOT NULL,
                        [SubmittedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_QuizResults] PRIMARY KEY ([QuizResultId]),
                        CONSTRAINT [FK_QuizResults_Quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [Quizzes] ([QuizId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_QuizResults_Users_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
                    );
                END
            ");

            _context.QuizResults.Add(quizResult);
            await _context.SaveChangesAsync();

            // Cập nhật lại Progress trong Enrollment cho học viên đối với khóa học này
            var quizObj = await _context.Quizzes.FindAsync(quizId);
            if (quizObj != null)
            {
                var enrollment = await _context.Enrollment
                    .FirstOrDefaultAsync(e => e.StudentId == student.UserId && e.CourseId == quizObj.CourseId);

                if (enrollment != null)
                {
                    var course = await _context.Course
                        .Include(c => c.Modules!).ThenInclude(m => m.Contents)
                        .Include(c => c.Modules!).ThenInclude(m => m.Quizzes)
                        .Include(c => c.Quizzes)
                        .FirstOrDefaultAsync(c => c.CourseId == quizObj.CourseId);

                    if (course != null)
                    {
                        var allContents = course.Modules?.SelectMany(m => m.Contents ?? new List<Content>()).ToList() ?? new List<Content>();
                        var allQuizzes = new List<Quiz>();
                        if (course.Modules != null) foreach (var m in course.Modules) if (m.Quizzes != null) allQuizzes.AddRange(m.Quizzes);
                        if (course.Quizzes != null) foreach (var q in course.Quizzes) if (!allQuizzes.Any(ex => ex.QuizId == q.QuizId)) allQuizzes.Add(q);

                        int totalItems = allContents.Count + allQuizzes.Count;
                        if (totalItems > 0)
                        {
                            var contentIds = allContents.Select(c => c.ContentId).ToList();
                            var quizIds = allQuizzes.Select(q => q.QuizId).ToList();

                            int completedContents = await _context.UserContentCompletions
                                .Where(uc => uc.StudentId == student.UserId && contentIds.Contains(uc.ContentId))
                                .CountAsync();

                            int completedQuizzes = await _context.QuizResults
                                .Where(qr => qr.StudentId == student.UserId && quizIds.Contains(qr.QuizId))
                                .Select(qr => qr.QuizId)
                                .Distinct()
                                .CountAsync();

                            enrollment.Progress = Math.Round((double)(completedContents + completedQuizzes) / totalItems * 100, 1);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            // Lưu câu trả lời của học viên vào TempData để hiển thị ở trang Result
            TempData["StudentAnswers"] = System.Text.Json.JsonSerializer.Serialize(studentAnswers);

            return RedirectToAction(nameof(Result), new { id = quizResult.QuizResultId });
        }

        // Trang hiển thị Kết quả làm bài (Quiz Result)
        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var quizResult = await _context.QuizResults
                .Include(r => r.Quiz!)
                    .ThenInclude(q => q.Questions)
                .Include(r => r.Quiz!)
                    .ThenInclude(q => q.Course)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.QuizResultId == id);

            if (quizResult == null) return NotFound();

            if (TempData["StudentAnswers"] != null)
            {
                ViewBag.StudentAnswers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, string>>((string)TempData["StudentAnswers"]!);
            }

            return View(quizResult);
        }
    }
}
