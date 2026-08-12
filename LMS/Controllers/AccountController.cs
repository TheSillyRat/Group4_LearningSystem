using System.Security.Claims;
using LMS.Data;
using LMS.Models;
using LMS.Services.Interfaces;
using LMS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToRoleDashboard();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var inputEmail = model.Email?.Trim().ToLower();
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == inputEmail);

            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Invalid email address or password.");
                return View(model);
            }

            var roleName = user.Role?.RoleName ?? "Student";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToRoleDashboard(roleName);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToRoleDashboard();
            }

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (existingUser)
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
                return View(model);
            }

            var studentRole = await _context.Role.FirstOrDefaultAsync(r => r.RoleName == "Student")
                              ?? await _context.Role.FirstAsync();

            var newUser = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                SecurityPassword = "123456",
                RoleId = studentRole.RoleId
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Account created successfully! Please sign in.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel { Step = 1 });
        }

        // POST: /Account/SendResetCode (Step 1)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResetCode(ForgotPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", "Please enter your Email address.");
                model.Step = 1;
                return View("ForgotPassword", model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email address does not exist in the system.");
                model.Step = 1;
                return View("ForgotPassword", model);
            }

            // Generate 6-digit OTP code
            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            user.ResetCode = otpCode;
            user.ResetCodeExpiry = DateTime.Now.AddMinutes(15);

            await _context.SaveChangesAsync();

            // Send real email via SMTP
            await _emailService.SendPasswordResetCodeAsync(user.Email, otpCode);

            ViewBag.SuccessMessage = $"Verification code has been sent to {user.Email}. Please check your email inbox.";

            model.Step = 2;
            return View("ForgotPassword", model);
        }

        // POST: /Account/ResetPasswordWithCode (Step 2)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordWithCode(ForgotPasswordViewModel model)
        {
            model.Step = 2;

            if (string.IsNullOrEmpty(model.Code))
            {
                ModelState.AddModelError("Code", "Please enter the 6-digit verification code.");
                return View("ForgotPassword", model);
            }

            if (string.IsNullOrEmpty(model.NewPassword) || model.NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "New password must be at least 6 characters long.");
                return View("ForgotPassword", model);
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError("ConfirmNewPassword", "New Password confirmation does not match.");
                return View("ForgotPassword", model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User account not found.");
                model.Step = 1;
                return View("ForgotPassword", model);
            }

            if (string.IsNullOrEmpty(user.ResetCode) || user.ResetCode != model.Code.Trim())
            {
                ModelState.AddModelError("Code", "Invalid verification code. Please check and try again.");
                return View("ForgotPassword", model);
            }

            if (!user.ResetCodeExpiry.HasValue || user.ResetCodeExpiry.Value < DateTime.Now)
            {
                ModelState.AddModelError("Code", "Verification code has expired. Please request a new code.");
                return View("ForgotPassword", model);
            }

            // Reset password
            user.Password = model.NewPassword;
            user.ResetCode = null;
            user.ResetCodeExpiry = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password reset successfully! Please sign in with your new password.";
            return RedirectToAction(nameof(Login));
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper Method: Role-Based Redirection
        private IActionResult RedirectToRoleDashboard(string? roleName = null)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                roleName = User.FindFirstValue(ClaimTypes.Role);
            }

            return roleName switch
            {
                "Admin" => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                "Instructor" => RedirectToAction("Index", "Assignment", new { area = "Instructor" }),
                "Student" => RedirectToAction("Index", "Assignment", new { area = "Student" }),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}
