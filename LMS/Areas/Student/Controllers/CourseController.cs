using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Student.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
