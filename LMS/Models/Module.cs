using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("Module")]
    public class Module
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(150)]
        public string ModuleName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        public Course? Course { get; set; }

        public ICollection<Content>? Contents { get; set; }

        public ICollection<Quiz>? Quizzes { get; set; }
    }
}