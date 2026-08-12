using System;

namespace LMS.Models
{
    public class QuizResult
    {
        public int QuizResultId { get; set; }

        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        public int StudentId { get; set; }
        public User? Student { get; set; }

        public double Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
