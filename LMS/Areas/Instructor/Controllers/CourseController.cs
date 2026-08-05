using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Instructor.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
