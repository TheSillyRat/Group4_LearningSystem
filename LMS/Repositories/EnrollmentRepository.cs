using LMS.Data;
using LMS.Models;
using LMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddEnrollmentAsync(Enrollment enrollment)
        {
            await _context.Enrollment.AddAsync(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task<Enrollment?> GetEnrollmentAsync(int studentId, int courseId)
        {
            return await _context.Enrollment
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
        {
            return await _context.Enrollment
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();
        }

        public async Task RemoveEnrollmentAsync(Enrollment enrollment)
        {
            _context.Enrollment.Remove(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEnrollmentAsync(Enrollment enrollment)
        {
            _context.Enrollment.Update(enrollment);
            await _context.SaveChangesAsync();
        }
    }
}
