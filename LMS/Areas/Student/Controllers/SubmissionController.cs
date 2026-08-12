using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Student.Controllers
{
    [Area("Student")]
    public class SubmissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubmissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int assignmentId, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "submissions");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var submission = new Submission
                {
                    AssignmentId = assignmentId,
                    StudentId = 3,
                    SubmitDate = DateTime.Now,
                    FilePath = "/uploads/submissions/" + uniqueFileName
                };

                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyAssignments));
        }

        [HttpGet]
        public async Task<IActionResult> MyAssignments()
        {
            var submissions = await _context.Submissions
                .Include(s => s.Assignment)
                .Where(s => s.StudentId == 3)
                .ToListAsync();

            return View(submissions);
        }

        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.SubmissionId == id && s.StudentId == 3);

            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }
    }
}
