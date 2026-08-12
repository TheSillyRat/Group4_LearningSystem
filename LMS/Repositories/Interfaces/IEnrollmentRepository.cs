using LMS.Models;

namespace LMS.Repositories.Interfaces
{
    public interface IEnrollmentRepository
    {
        Task AddEnrollmentAsync(Enrollment enrollment);
        Task RemoveEnrollmentAsync(Enrollment enrollment);
        Task<Enrollment?> GetEnrollmentAsync(int studentId, int courseId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);
        Task UpdateEnrollmentAsync(Enrollment enrollment);
    }
}
