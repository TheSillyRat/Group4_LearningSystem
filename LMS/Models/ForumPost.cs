using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("ForumPost")]
    public class ForumPost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string? Content { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public Course? Course { get; set; }

        public User? User { get; set; }

        public ICollection<ForumReply>? Replies { get; set; }
    }
}