using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Instructor.Controllers
{
    public class AssessmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
