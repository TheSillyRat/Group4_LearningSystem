using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("Quizzes")]
    public class Quiz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuizId { get; set; }

        [Required]
        [StringLength(150)]
        public string QuizTitle { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime OpenDate { get; set; }

        public DateTime CloseDate { get; set; }

        public int Duration { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        public Course? Course { get; set; }

        public ICollection<Question>? Questions { get; set; }

        [ForeignKey("Module")]
        public int? ModuleId { get; set; }

        public Module? Module { get; set; }
    }
}