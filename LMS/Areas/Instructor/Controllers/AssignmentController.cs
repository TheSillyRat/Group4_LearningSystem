using LMS.Models;
using LMS.Repositories.Interfaces;
using LMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IAssignmentRepository _assignmentRepository;

        public AssignmentController(IAssignmentService assignmentService, IAssignmentRepository assignmentRepository)
        {
            _assignmentService = assignmentService;
            _assignmentRepository = assignmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var assignments = await _assignmentRepository.GetAllAssignmentsAsync();
            return View(assignments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assignment assignment)
        {
            ModelState.Remove("Course");
            ModelState.Remove("Instructor");
            ModelState.Remove("Submissions");
            if (!ModelState.IsValid)
            {
                return View(assignment);
            }

            await _assignmentRepository.AddAssignmentAsync(assignment);
            return RedirectToAction(nameof(Details), new { id = assignment.AssignmentId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var assignment = await _assignmentRepository.GetAssignmentByIdAsync(id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int submissionId, int assignmentId, double score, string feedback)
        {
            var result = await _assignmentService.GradeSubmissionAsync(submissionId, score, feedback);

            if (!result)
            {
                TempData["ErrorMessage"] = "Invalid score or submission not found.";
            }
            else
            {
                TempData["SuccessMessage"] = "Graded successfully!";
            }

            return RedirectToAction(nameof(Details), new { id = assignmentId });
        }


    }
}
