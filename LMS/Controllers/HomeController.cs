using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var roleName = User.FindFirstValue(ClaimTypes.Role);
                return roleName switch
                {
                    "Admin" => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                    "Instructor" => RedirectToAction("Index", "Assignment", new { area = "Instructor" }),
                    "Student" => RedirectToAction("Index", "Assignment", new { area = "Student" }),
                    _ => RedirectToAction("Login", "Account")
                };
            }

            return RedirectToAction("Login", "Account");
        }
    }
}
