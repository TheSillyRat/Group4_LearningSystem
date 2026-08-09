using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Student.Controllers
{
    [Area("Student")]
    public class ForumController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
