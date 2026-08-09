using LMS.Models;

namespace LMS.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<bool> EnrollStudentAsync(int studentId, int courseId);
        Task<bool> CancelEnrollmentAsync(int studentId, int courseId);
        Task<bool> IsEnrolledAsync(int studentId, int courseId);
        Task<IEnumerable<Enrollment>> GetMyCoursesAsync(int studentId);
    }
}
