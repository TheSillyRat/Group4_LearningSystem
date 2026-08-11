using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/User
        [HttpGet]
        public async Task<IActionResult> Index(string? searchString, int? roleId)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.FullName.Contains(searchString) || u.Email.Contains(searchString));
            }

            if (roleId.HasValue && roleId.Value > 0)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            ViewBag.Roles = new SelectList(await _context.Role.ToListAsync(), "RoleId", "RoleName", roleId);
            ViewBag.SearchString = searchString;

            var users = await query.OrderByDescending(u => u.UserId).ToListAsync();
            return View(users);
        }

        // GET: /Admin/User/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = new SelectList(await _context.Role.ToListAsync(), "RoleId", "RoleName");
            return View();
        }

        // POST: /Admin/User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(await _context.Role.ToListAsync(), "RoleId", "RoleName", user.RoleId);
                return View(user);
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This Email address is already in use.");
                ViewBag.Roles = new SelectList(await _context.Role.ToListAsync(), "RoleId", "RoleName", user.RoleId);
                return View(user);
            }

            if (string.IsNullOrEmpty(user.SecurityPassword))
            {
                user.SecurityPassword = "123456";
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/User/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = new SelectList(await _context.Role.ToListAsync(), "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // POST: /Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(await _context.Role.ToListAsync(), "RoleId", "RoleName", user.RoleId);
                return View(user);
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.RoleId = user.RoleId;

            if (!string.IsNullOrEmpty(user.Password))
            {
                existingUser.Password = user.Password;
            }

            if (!string.IsNullOrEmpty(user.SecurityPassword))
            {
                existingUser.SecurityPassword = user.SecurityPassword;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
