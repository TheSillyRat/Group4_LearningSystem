using LMS.Models;

namespace LMS.Repositories.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<IEnumerable<Assignment>> GetAllAssignmentsAsync();
        Task<Assignment?> GetAssignmentByIdAsync(int id);
        Task AddAssignmentAsync(Assignment assignment);
        Task UpdateAssignmentAsync(Assignment assignment);
        Task DeleteAssignmentAsync(Assignment assignment);

        Task<Submission?> GetSubmissionByIdAsync(int id);
        Task UpdateSubmissionAsync(Submission submission);
    }
}