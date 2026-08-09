using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Models;

namespace LMS.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class ModuleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModuleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Form tạo Module mới cho 1 Course (nhận vào courseId)
        public IActionResult Create(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Module module)
        {
            if (ModelState.IsValid)
            {
                _context.Module.Add(module);
                await _context.SaveChangesAsync();
                // Chuyển hướng về xem chi tiết khóa học chứa module này
                return RedirectToAction("Details", "Course", new { id = module.CourseId });
            }
            ViewBag.CourseId = module.CourseId;
            return View(module);
        }

        // Form Sửa Module
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var module = await _context.Module.FindAsync(id);
            if (module == null) return NotFound();

            return View(module);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Module module)
        {
            if (id != module.ModuleId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(module);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Course", new { id = module.CourseId });
            }
            return View(module);
        }

        // Xóa Module
        public async Task<IActionResult> Delete(int id)
        {
            var module = await _context.Module.FindAsync(id);
            if (module != null)
            {
                int courseId = module.CourseId;
                _context.Module.Remove(module);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Course", new { id = courseId });
            }
            return NotFound();
        }
    }
}