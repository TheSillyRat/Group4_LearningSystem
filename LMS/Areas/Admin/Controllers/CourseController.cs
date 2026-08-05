using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Admin.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
