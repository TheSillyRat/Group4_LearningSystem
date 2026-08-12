using LMS.Models;

namespace LMS.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<bool> EnrollStudentAsync(int studentId, int courseId);
        Task<bool> CancelEnrollmentAsync(int studentId, int courseId);
        Task<bool> IsEnrolledAsync(int studentId, int courseId);
        Task<IEnumerable<Enrollment>> GetMyCoursesAsync(int studentId);
        Task<bool> MarkAttendanceAsync(int studentId, int courseId);
        Task<bool> IncrementProgressAsync(int studentId, int courseId);
        Task<Enrollment?> GetEnrollmentAsync(int studentId, int courseId);
    }
}
