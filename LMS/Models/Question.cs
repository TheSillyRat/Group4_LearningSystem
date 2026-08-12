using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("Questions")]
    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(1000)]
        public string QuestionText { get; set; } = string.Empty; // Nội dung câu hỏi

        [Required]
        [StringLength(50)]
        public string QuestionType { get; set; } = "MultipleChoice"; // "MultipleChoice" (Trắc nghiệm) hoặc "Essay" (Tự luận)

        // Các lựa chọn dành cho câu hỏi Trắc nghiệm
        [StringLength(300)]
        public string? OptionA { get; set; }

        [StringLength(300)]
        public string? OptionB { get; set; }

        [StringLength(300)]
        public string? OptionC { get; set; }

        [StringLength(300)]
        public string? OptionD { get; set; }

        // Đáp án đúng ("A", "B", "C", "D" đối với Trắc nghiệm, hoặc Gợi ý đáp án đối với Tự luận)
        [StringLength(500)]
        public string? CorrectAnswer { get; set; }

        [ForeignKey("Quiz")]
        public int QuizId { get; set; }

        public Quiz? Quiz { get; set; }
    }
}