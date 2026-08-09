using System.Data;
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
            public CourseController(ApplicationDbContext context)   //tạo hàm dựng
            {
                _context = context;
            }

            //danh sách Course
            public async Task<IActionResult> Index()
            {
                var courses = await _context.Course.Include(c => c.Instructor).ToListAsync();   //lấy tất cả danh sách khoá học
                return View(courses);
            }

            //chi tiết Course 
            public async Task<IActionResult> Details(int? id)
            {
                if (id == null) return NotFound();

                var course = await _context.Course.Include(c => c.Modules!).ThenInclude(m => m.Contents).Include(c => c.Quizzes).FirstOrDefaultAsync(m => m.CourseId == id);
                if (course == null) return NotFound();

                return View(course);
            }

            //tạo mới Course
            [HttpGet]
            public IActionResult Create()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Course course)
            {
                if (ModelState.IsValid)
                {
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
            public async Task<IActionResult> Edit(int id, Course course)
            {
                if (id != course.CourseId) return NotFound();

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(course);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.Course.Any(e => e.CourseId == course.CourseId))
                            return NotFound();
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
