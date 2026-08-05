using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Instructor.Controllers
{
    public class AssignmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
