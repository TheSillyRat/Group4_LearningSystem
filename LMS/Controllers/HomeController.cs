using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
