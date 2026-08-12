using LMS.Models;
using LMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentRepository _assignmentRepository;

        public AssignmentController(IAssignmentRepository assignmentRepository)
        {
            _assignmentRepository = assignmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var assignments = await _assignmentRepository.GetAllAssignmentsAsync();
            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var assignment = await _assignmentRepository.GetAssignmentByIdAsync(id);
            if (assignment == null) return NotFound();
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitWork(int assignmentId, int studentId, string filePath)
        {
            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                FilePath = filePath,
                SubmitDate = DateTime.Now
            };

            var assignment = await _assignmentRepository.GetAssignmentByIdAsync(assignmentId);
            if (assignment != null)
            {
                if (assignment.Submissions == null)
                {
                    assignment.Submissions = new List<Submission>();
                }
                assignment.Submissions.Add(submission);
                await _assignmentRepository.UpdateAssignmentAsync(assignment);
            }

            return RedirectToAction(nameof(Details), new { id = assignmentId });
        }
    }
}
