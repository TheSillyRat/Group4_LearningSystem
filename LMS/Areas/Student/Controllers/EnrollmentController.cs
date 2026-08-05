using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Student.Controllers
{
    public class EnrollmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
