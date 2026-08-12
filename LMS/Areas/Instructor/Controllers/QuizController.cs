using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Models;

namespace LMS.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Form tạo Quiz mới cho 1 Course
        public async Task<IActionResult> Create(int courseId, int? moduleId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.Modules = await _context.Module.Where(m => m.CourseId == courseId).ToListAsync();

            var quiz = new Quiz
            {
                CourseId = courseId,
                ModuleId = moduleId,
                OpenDate = DateTime.Now,
                CloseDate = DateTime.Now.AddDays(7),
                Duration = 45 // 45 phút mặc định
            };
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quiz quiz)
        {
            ModelState.Remove("Course");
            ModelState.Remove("Module");
            ModelState.Remove("OpenDate");
            ModelState.Remove("CloseDate");

            if (ModelState.IsValid)
            {
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Course", new { id = quiz.CourseId });
            }
            ViewBag.CourseId = quiz.CourseId;
            ViewBag.Modules = await _context.Module.Where(m => m.CourseId == quiz.CourseId).ToListAsync();
            return View(quiz);
        }

        // Xóa Quiz
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                int courseId = quiz.CourseId;
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Course", new { id = courseId });
            }
            return NotFound();
        }

        // 1. Xem Chi tiết Quiz & Danh sách câu hỏi (GET)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Course)
                .Include(q => q.Module)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null) return NotFound();

            return View(quiz);
        }

        // 2. Thêm câu hỏi mới vào Quiz (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(Question question)
        {
            ModelState.Remove("Quiz");

            if (ModelState.IsValid)
            {
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = question.QuizId });
            }

            return RedirectToAction(nameof(Details), new { id = question.QuizId });
        }

        // 3. Xóa câu hỏi (POST/GET)
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                int quizId = question.QuizId;
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = quizId });
            }
            return NotFound();
        }
    }
}