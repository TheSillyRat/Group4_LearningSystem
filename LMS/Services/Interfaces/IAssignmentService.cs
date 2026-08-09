using LMS.Models;

namespace LMS.Services.Interfaces
{
    public interface IAssignmentService
    {
        Task<bool> GradeSubmissionAsync(int submissionId, double score, string feedback);
    }
}