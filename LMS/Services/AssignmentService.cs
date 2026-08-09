using LMS.Repositories.Interfaces;
using LMS.Services.Interfaces;

namespace LMS.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAssignmentRepository _repository;

        public AssignmentService(IAssignmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> GradeSubmissionAsync(int submissionId, double score, string feedback)
        {
            if (score < 0 || score > 10)
            {
                return false;
            }

            var submission = await _repository.GetSubmissionByIdAsync(submissionId);

            if (submission == null)
            {
                return false;
            }

            submission.Score = score;
            submission.Feedback = feedback;

            await _repository.UpdateSubmissionAsync(submission);

            return true;
        }
    }
}