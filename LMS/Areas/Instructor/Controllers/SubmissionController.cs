using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class SubmissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubmissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int assignmentId)
        {
            var submissions = await _context.Submissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentId == assignmentId)
                .ToListAsync();

            var assignment = await _context.Assignments.FindAsync(assignmentId);
            ViewBag.AssignmentTitle = assignment?.AssignmentTitle ?? "Assignment";

            return View(submissions);
        }

        [HttpGet]
        public async Task<IActionResult> Grade(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.SubmissionId == id);

            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int id, double score, string feedback)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null) return NotFound();

            submission.Score = score;
            submission.Feedback = feedback;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home", new { area = "Instructor" });
        }
    }
}