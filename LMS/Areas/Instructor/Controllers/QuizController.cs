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
        public IActionResult Create(int courseId)
        {
            ViewBag.CourseId = courseId;
            var quiz = new Quiz
            {
                CourseId = courseId,
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
            if (ModelState.IsValid)
            {
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Course", new { id = quiz.CourseId });
            }
            ViewBag.CourseId = quiz.CourseId;
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
    }
}