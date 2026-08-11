using LMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
    }
}
