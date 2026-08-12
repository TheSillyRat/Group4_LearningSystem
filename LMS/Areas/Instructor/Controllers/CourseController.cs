using System.Data;
using System.Security.Claims;
using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Instructor.Controllers
{
        [Area("Instructor")]
        public class CourseController : Controller
        {
            private readonly ApplicationDbContext _context;         //khai báo DI
            private readonly IWebHostEnvironment _env;

        public CourseController(ApplicationDbContext context, IWebHostEnvironment env)   //tạo hàm dựng
            {
                _context = context;
                _env = env;
            }

        // Danh sách Khóa học DO CHÍNH GIẢNG VIÊN ĐANG ĐĂNG NHẬP TẠO
        public async Task<IActionResult> Index()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;

            var currentInstructor = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.FullName == userName);

            if (currentInstructor == null)
            {
                return View(new List<Course>());
            }

            // Lọc ra các Khóa học có InstructorId trùng với UserId của Giảng viên hiện tại
            var courses = await _context.Course
                .Include(c => c.Instructor)
                .Where(c => c.InstructorId == currentInstructor.UserId)
                .ToListAsync();

            return View(courses);
        }

        // Chi tiết Course 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Course
                .Include(c => c.Modules!)
                    .ThenInclude(m => m.Contents)
                .Include(c => c.Modules!)
                    .ThenInclude(m => m.Quizzes)
                .Include(c => c.Quizzes!)
                    .ThenInclude(q => q.Module)
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null) return NotFound();

            return View(course);
        }

        // Tạo mới Course
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile? imageFile)
        {
            ModelState.Remove("Instructor");
            ModelState.Remove("ImageUrl");

            // Xử lý kiểm tra file ảnh
            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "The image size must be under 5MB!");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(imageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", "Only image format (.jpg, .jpeg, .png, .gif, .webp)!");
                }

                if (ModelState.IsValid)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "courses");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    course.ImageUrl = "/uploads/courses/" + uniqueFileName;
                }
            }

            if (ModelState.IsValid)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
                var currentInstructor = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail || u.FullName == userName);

                if (currentInstructor != null)
                {
                    course.InstructorId = currentInstructor.UserId;
                }
                else
                {
                    var firstInstructor = await _context.Users.FirstAsync();
                    course.InstructorId = firstInstructor.UserId;
                }

                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        //chỉnh sửa Course
        [HttpGet]
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null) return NotFound();

                var course = await _context.Course.FindAsync(id);
                if (course == null) return NotFound();

                return View(course);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course, IFormFile? imageFile)
        {
            if (id != course.CourseId) return NotFound();

            ModelState.Remove("Instructor");

            if (imageFile != null && imageFile.Length > 0)
            {
                // 1. Kiểm tra dung lượng dưới 5MB
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "The image size must be under 5MB!");
                }

                // 2. Kiểm tra định dạng ảnh
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(imageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", "Only image format (.jpg, .jpeg, .png, .gif, .webp)!");
                }

                if (ModelState.IsValid)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "courses");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(course.ImageUrl) && course.ImageUrl.StartsWith("/uploads/courses/"))
                    {
                        string oldPath = Path.Combine(_env.WebRootPath, course.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    course.ImageUrl = "/uploads/courses/" + uniqueFileName;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Course.Any(e => e.CourseId == course.CourseId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        //xóa Course
        [HttpGet]
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null) return NotFound();
                var course = await _context.Course.FirstOrDefaultAsync(m => m.CourseId == id);
                if (course == null) return NotFound();

                return View(course);
            }

            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var course = await _context.Course.FindAsync(id);
                if (course != null)
                {
                    _context.Course.Remove(course);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
        }
}
