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
        public async Task<IActionResult> Create(int moduleId)
        {
            ViewBag.ModuleId = moduleId;

            // Tính vị trí DisplayOrder lớn nhất hiện tại thuộc Module này + 1
            int maxOrder = await _context.Content
                .Where(c => c.ModuleId == moduleId)
                .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;

            var content = new Content
            {
                ModuleId = moduleId,
                DisplayOrder = maxOrder + 1
            };

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)] // 1 GB
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        public async Task<IActionResult> Create(Content content, IFormFile? fileUpload)
        {
            ModelState.Remove("FileUrl");
            ModelState.Remove("Module");

            if (fileUpload == null || fileUpload.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload!");
            }

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

                    // Chuẩn hóa tên file sạch (loại bỏ tiếng Việt có dấu và ký tự đặc biệt gây lỗi 404 khi stream Video)
                    string ext = Path.GetExtension(fileUpload.FileName).ToLower();
                    string safeFileName = System.Text.RegularExpressions.Regex.Replace(Path.GetFileNameWithoutExtension(fileUpload.FileName), @"[^a-zA-Z0-9_\-]", "_");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + safeFileName + ext;
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

        // Form Sửa Content (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var content = await _context.Content.FindAsync(id);
            if (content == null) return NotFound();

            return View(content);
        }

        // Xử lý Sửa Content (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)] // 1 GB
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        public async Task<IActionResult> Edit(int id, Content content, IFormFile? fileUpload)
        {
            if (id != content.ContentId) return NotFound();

            ModelState.Remove("FileUrl");
            ModelState.Remove("Module");

            if (ModelState.IsValid)
            {
                // Nếu chọn Upload file mới thay thế
                if (fileUpload != null && fileUpload.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string ext = Path.GetExtension(fileUpload.FileName).ToLower();
                    string safeFileName = System.Text.RegularExpressions.Regex.Replace(Path.GetFileNameWithoutExtension(fileUpload.FileName), @"[^a-zA-Z0-9_\-]", "_");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + safeFileName + ext;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(stream);
                    }

                    // Xóa file cũ trong thư mục uploads nếu có
                    if (!string.IsNullOrEmpty(content.FileUrl) && content.FileUrl.StartsWith("/uploads/"))
                    {
                        string oldPath = Path.Combine(_env.WebRootPath, content.FileUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    content.FileUrl = "/uploads/" + uniqueFileName;
                }

                _context.Update(content);
                await _context.SaveChangesAsync();

                var module = await _context.Module.FindAsync(content.ModuleId);
                return RedirectToAction("Details", "Course", new { id = module?.CourseId });
            }

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