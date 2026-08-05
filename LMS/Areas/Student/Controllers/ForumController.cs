using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Student.Controllers
{
    public class ForumController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
