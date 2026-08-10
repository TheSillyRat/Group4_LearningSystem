using LMS.Data;
using LMS.Models;
using LMS.Repositories.Interfaces;
using LMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ApplicationDbContext _context; //Thêm DbContext


        public AssignmentController(IAssignmentService assignmentService, IAssignmentRepository assignmentRepository, ApplicationDbContext context)
        {
            _assignmentService = assignmentService;
            _assignmentRepository = assignmentRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var assignments = await _assignmentRepository.GetAllAssignmentsAsync();
            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Courses = await _context.Course.ToListAsync();
            ViewBag.Modules = await _context.Module.ToListAsync();
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
                ViewBag.Courses = await _context.Course.ToListAsync();
                ViewBag.Modules = await _context.Module.ToListAsync();
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

        // API lấy danh sách Module theo CourseId dạng JSON
        [HttpGet]
        public async Task<IActionResult> GetModulesByCourse(int courseId)
        {
            var modules = await _context.Module
                .Where(m => m.CourseId == courseId)
                .Select(m => new { id = m.ModuleId, name = m.ModuleName })
                .ToListAsync();

            return Json(modules);
        }


    }
}