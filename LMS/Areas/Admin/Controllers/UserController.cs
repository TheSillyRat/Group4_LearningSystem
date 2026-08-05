using Microsoft.AspNetCore.Mvc;

namespace LMS.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
