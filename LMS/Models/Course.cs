using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }

        [Required]
        [StringLength(150)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(300)]
        public string? Prerequisite { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Materials { get; set; }

        [ForeignKey("Instructor")]
        public int InstructorId { get; set; }

        public User? Instructor { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }

        public ICollection<Module>? Modules { get; set; }

        public ICollection<Assignment>? Assignments { get; set; }

        public ICollection<ForumPost>? ForumPosts { get; set; }
        public ICollection<Quiz>? Quizzes { get; set; }
    }
}