using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("Assignment")]
    public class Assignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssignmentId { get; set; }

        [Required]
        [StringLength(200)]
        public string AssignmentTitle { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }
        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        public DateTime DueDate { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [ForeignKey("Instructor")]
        public int InstructorId { get; set; }

        public Course? Course { get; set; }

        public User? Instructor { get; set; }
        public bool IsPublished { get; set; } = true;

        public ICollection<Submission>? Submissions { get; set; }
    }
}