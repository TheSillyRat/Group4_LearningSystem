using LMS.Models;
using LMS.Repositories.Interfaces;
using LMS.Services.Interfaces;

namespace LMS.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;

        public EnrollmentService(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<bool> CancelEnrollmentAsync(int studentId, int courseId)
        {
            var enrollment = await _enrollmentRepository.GetEnrollmentAsync(studentId, courseId);
            if (enrollment == null)
                return false;

            await _enrollmentRepository.RemoveEnrollmentAsync(enrollment);
            return true;
        }

        public async Task<bool> EnrollStudentAsync(int studentId, int courseId)
        {
            var existingEnrollment = await _enrollmentRepository.GetEnrollmentAsync(studentId, courseId);
            if (existingEnrollment != null)
                return false; // Already enrolled

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now,
                Progress = 0,
                Attendance = false
            };

            await _enrollmentRepository.AddEnrollmentAsync(enrollment);
            return true;
        }

        public async Task<IEnumerable<Enrollment>> GetMyCoursesAsync(int studentId)
        {
            return await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
        }

        public async Task<bool> IsEnrolledAsync(int studentId, int courseId)
        {
            var enrollment = await _enrollmentRepository.GetEnrollmentAsync(studentId, courseId);
            return enrollment != null;
        }
    }
}
