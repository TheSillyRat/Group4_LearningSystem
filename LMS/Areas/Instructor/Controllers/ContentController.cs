using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Models;

namespace LMS.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class ContentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ContentController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Form thêm tài liệu/bài học vào Module
        public IActionResult Create(int moduleId)
        {
            ViewBag.ModuleId = moduleId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Content content, IFormFile? fileUpload)
        {
            if (ModelState.IsValid)
            {
                // Xử lý Upload File (PDF, Slide, docx...)
                if (fileUpload != null && fileUpload.Length > 0)
                {
                    // Tạo thư mục wwwroot/uploads nếu chưa có
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Đổi tên file tránh trùng lắp
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileUpload.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn tương đối vào CSDL
                    content.FileUrl = "/uploads/" + uniqueFileName;
                }

                _context.Content.Add(content);
                await _context.SaveChangesAsync();

                // Lấy CourseId để quay lại trang Details của Course
                var module = await _context.Module.FindAsync(content.ModuleId);
                return RedirectToAction("Details", "Course", new { id = module?.CourseId });
            }

            ViewBag.ModuleId = content.ModuleId;
            return View(content);
        }

        // Xóa Content
        public async Task<IActionResult> Delete(int id)
        {
            var content = await _context.Content.FindAsync(id);
            if (content != null)
            {
                var module = await _context.Module.FindAsync(content.ModuleId);

                // Nếu muốn xóa file vật lý trong wwwroot khi xóa DB:
                if (!string.IsNullOrEmpty(content.FileUrl))
                {
                    string fullPath = Path.Combine(_env.WebRootPath, content.FileUrl.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                }

                _context.Content.Remove(content);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Course", new { id = module?.CourseId });
            }
            return NotFound();
        }
    }
}