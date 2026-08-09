using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
